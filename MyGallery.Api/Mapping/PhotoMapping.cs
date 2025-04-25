using MyGallery.Api.DTOs;
using MyGallery.Api.Entities;
namespace MyGallery.Api.Mapping
{
    public static class PhotoMapping
    {
        public static Photo ToEntity(this CreatePhotoDTO createPhotoDTO)
        {
            return new Photo
            {
                ImageUrl = createPhotoDTO.ImageUrl,
                CategoryId = createPhotoDTO.CategoryId
            };
        }

        public static PhotoDTO ToPhotoDTO(this Photo photo)
        {
            return new PhotoDTO(
                photo.Id,
                photo.ImageUrl,
                photo.CategoryId,
                photo.Category?.Name ?? "Unknown Category"
            );
        }
    }
}
