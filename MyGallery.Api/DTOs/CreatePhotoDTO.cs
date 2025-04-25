using System.ComponentModel.DataAnnotations;

namespace MyGallery.Api.DTOs
{
    public record class CreatePhotoDTO(
        [Required] string ImageUrl,
        int CategoryId

        );
   
}
