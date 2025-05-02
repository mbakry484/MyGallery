using Microsoft.AspNetCore.Mvc;
using MyGallery.Api.Data;
using MyGallery.Api.Entities;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MyGallery.Api.Controllers
{
    public class HomeController : Controller
    {
        private readonly MyGalleryContext _context;

        public HomeController(MyGalleryContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var categories = await _context.Category.ToListAsync();
                return View(categories);
            }
            catch (Exception ex)
            {
                // For debugging purposes
                Console.WriteLine($"Error in Index action: {ex.Message}");
                return View(new List<Category>());
            }
        }
    }
}