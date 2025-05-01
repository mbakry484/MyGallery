using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MyGallery.Api.DTOs
{
    public record class CreatePhotoDTO(
        [Required] IFormFile ImageFile,
        int CategoryId
    );
}
