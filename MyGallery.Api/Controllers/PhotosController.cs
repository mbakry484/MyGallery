using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyGallery.Api.Data;
using MyGallery.Api.DTOs;
using MyGallery.Api.Entities;
using MyGallery.Api.Mapping;
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

        public PhotosController(MyGalleryContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PhotoDTO>>> GetAllPhotos()
        {
            var photos = await _context.Photo
                .Include(p => p.Category)
                .ToListAsync();

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

            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(newPhoto.ImageFile.FileName)}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await newPhoto.ImageFile.CopyToAsync(stream);
            }

            // Create photo entity
            var photo = new Photo
            {
                ImageUrl = $"/uploads/{fileName}",
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

            // If a new file was uploaded
            if (updatePhotoDto.ImageFile != null && updatePhotoDto.ImageFile.Length > 0)
            {
                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(updatePhotoDto.ImageFile.FileName)}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Save the file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await updatePhotoDto.ImageFile.CopyToAsync(stream);
                }

                // Update the image URL
                photo.ImageUrl = $"/uploads/{fileName}";
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
    }
}

