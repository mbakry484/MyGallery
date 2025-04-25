namespace MyGallery.Api.DTOs
{
    public class UpdatePhotoDTO
    {
        public string ImageUrl { get; set; } = string.Empty;
        public int CategoryId { get; set; }
    }
}