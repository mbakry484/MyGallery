namespace MyGallery.Api.DTOs
{
    public record PhotoDTO(
        int Id,
        string? ImageUrl,
        int CategoryId,
        string CategoryName
    );
}
