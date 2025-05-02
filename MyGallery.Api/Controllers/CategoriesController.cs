using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyGallery.Api.Data;
using MyGallery.Api.DTOs;
using MyGallery.Api.Entities;

namespace MyGallery.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly MyGalleryContext _context;

        public CategoriesController(MyGalleryContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetCategories()
        {
            var categories = await _context.Category.ToListAsync();

            // Log for debugging
            foreach (var category in categories)
            {
                Console.WriteLine($"Category: Id={category.Id}, Name={category.Name}");
            }

            var categoryDTOs = categories.Select(c => new CategoryDTO(c.Id, c.Name ?? "Unknown"));

            return Ok(categoryDTOs);
        }

        [HttpPost]
        public async Task<ActionResult<CategoryDTO>> CreateCategory(CreateCategoryDTO categoryDTO)
        {
            var category = new Category
            {
                Name = categoryDTO.Name
            };

            _context.Category.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, new CategoryDTO(category.Id, category.Name));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDTO>> GetCategory(int id)
        {
            var category = await _context.Category.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return new CategoryDTO(category.Id, category.Name);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCategory(int id)
        {
            var category = await _context.Category.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            // Check if any photos are using this category
            var hasPhotos = await _context.Photo.AnyAsync(p => p.CategoryId == id);
            if (hasPhotos)
            {
                return BadRequest("Cannot delete category because it is being used by one or more photos");
            }

            _context.Category.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}