using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyGallery.Api.Data;
using MyGallery.Api.DTOs;
using MyGallery.Api.Entities;
using MyGallery.Api.Mapping;
using MyGallery.Api.Services;
using System;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace MyGallery.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhotosController : ControllerBase
    {
        private readonly MyGalleryContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ImgBBService _imgBBService;

        public PhotosController(
            MyGalleryContext context,
            IWebHostEnvironment environment,
            ImgBBService imgBBService)
        {
            _context = context;
            _environment = environment;
            _imgBBService = imgBBService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PhotoDTO>>> GetAllPhotos([FromQuery] int? categoryId = null)
        {
            IQueryable<Photo> query = _context.Photo;

            // Filter by category if categoryId is provided
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // Include category information after filtering
            var photos = await query.Include(p => p.Category).ToListAsync();
            var photoDTOs = photos.Select(p => p.ToPhotoDTO());

            return Ok(photoDTOs);
        }

        [HttpPost]
        public async Task<ActionResult<PhotoDTO>> CreatePhoto([FromForm] CreatePhotoDTO newPhoto)
        {
            if (newPhoto.ImageFile == null || newPhoto.ImageFile.Length == 0)
            {
                return BadRequest("No file was uploaded");
            }

            // Verify the category exists
            var categoryExists = await _context.Category.AnyAsync(c => c.Id == newPhoto.CategoryId);
            if (!categoryExists)
            {
                return BadRequest($"Category with ID {newPhoto.CategoryId} does not exist");
            }

            try
            {
                // Upload to ImgBB instead of local storage
                var imageUrl = await _imgBBService.UploadImageAsync(newPhoto.ImageFile);

                // Create photo entity with the ImgBB URL
                var photo = new Photo
                {
                    ImageUrl = imageUrl,
                    CategoryId = newPhoto.CategoryId
                };

                // Add to database
                _context.Photo.Add(photo);
                await _context.SaveChangesAsync();

                // Retrieve the saved photo with category info for the response
                var savedPhoto = await _context.Photo
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == photo.Id);

                // Return created photo as DTO
                return CreatedAtAction(nameof(GetPhoto), new { id = photo.Id }, savedPhoto.ToPhotoDTO());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading image: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PhotoDTO>> GetPhoto(int id)
        {
            var photo = await _context.Photo
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (photo == null)
            {
                return NotFound();
            }

            return photo.ToPhotoDTO();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PhotoDTO>> UpdatePhoto(int id, [FromForm] UpdatePhotoDTO updatePhotoDto)
        {
            // Verify the photo exists
            var photo = await _context.Photo.FindAsync(id);
            if (photo == null)
            {
                return NotFound();
            }

            // Verify the category exists
            var categoryExists = await _context.Category.AnyAsync(c => c.Id == updatePhotoDto.CategoryId);
            if (!categoryExists)
            {
                return BadRequest($"Category with ID {updatePhotoDto.CategoryId} does not exist");
            }

            try
            {
                // If a new file was uploaded
                if (updatePhotoDto.ImageFile != null && updatePhotoDto.ImageFile.Length > 0)
                {
                    // Upload to ImgBB instead of local storage
                    var imageUrl = await _imgBBService.UploadImageAsync(updatePhotoDto.ImageFile);

                    // Update the image URL
                    photo.ImageUrl = imageUrl;
                }

                // Update category
                photo.CategoryId = updatePhotoDto.CategoryId;
                await _context.SaveChangesAsync();

                // Retrieve the updated photo with category info
                var updatedPhoto = await _context.Photo
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == id);

                return Ok(updatedPhoto.ToPhotoDTO());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating image: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<PhotoDTO>> DeletePhoto(int id)
        {
            var photo = await _context.Photo
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (photo == null)
            {
                return NotFound();
            }

            // Get the DTO before removing the entity
            var photoDto = photo.ToPhotoDTO();

            // Remove the photo
            _context.Photo.Remove(photo);
            await _context.SaveChangesAsync();

            return Ok(photoDto);
        }

        [HttpPost("test-imgbb")]
        public async Task<ActionResult<string>> TestImgBBUpload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file was uploaded");
            }

            try
            {
                // Test upload to ImgBB
                var imageUrl = await _imgBBService.UploadImageAsync(file);

                return Ok(new
                {
                    Message = "Image uploaded successfully to ImgBB",
                    Url = imageUrl
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Error = "Error uploading image to ImgBB",
                    Details = ex.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }

        [HttpGet("test-imgbb")]
        public IActionResult TestImgBBPage()
        {
            return File("~/imgbb-test.html", "text/html");
        }
    }
}

