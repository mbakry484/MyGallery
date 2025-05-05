using Microsoft.AspNetCore.Http;

namespace MyGallery.Api.DTOs
{
    public class UpdatePhotoDTO
    {
        public IFormFile? ImageFile { get; set; }
        public int CategoryId { get; set; }
    }
}