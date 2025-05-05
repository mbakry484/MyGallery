namespace MyGallery.Api.Entities;

public record class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Photo> Photos { get; set; } = new List<Photo>();
}
