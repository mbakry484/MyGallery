using MyGallery.Api.DTOs;
using MyGallery.Api.Entities;
namespace MyGallery.Api.Mapping
{
    public static class PhotoMapping
    {
        public static Photo ToEntity(this CreatePhotoDTO photo)
        {
            return new Photo
            {
                CategoryId = photo.CategoryId
            };
        }

        public static PhotoDTO ToPhotoDTO(this Photo photo)
        {
            return new PhotoDTO(
                photo.Id,
                photo.ImageUrl,
                photo.CategoryId,
                photo.Category?.Name ?? string.Empty
            );
        }
    }
}
