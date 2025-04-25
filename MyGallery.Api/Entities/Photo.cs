namespace MyGallery.Api.Entities;

public record class Photo
{
    public int Id { get; set; }
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
}
