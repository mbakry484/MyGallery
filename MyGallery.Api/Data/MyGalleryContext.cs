using System;
using Microsoft.EntityFrameworkCore;
using MyGallery.Api.Entities;
namespace MyGallery.Api.Data;


public class MyGalleryContext(DbContextOptions<MyGalleryContext> options) : DbContext(options)
{
    public DbSet<Photo> Photo => Set<Photo>();
    public DbSet<Category> Category => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(
        new { Id = 1, Name = "Animals" },
        new { Id = 2, Name = "Sunsets" },
        new { Id = 3, Name = "Insects" },
        new { Id = 4, Name = "Sky" },
        new { Id = 5, Name = "Randoms" }
        );

    }


};

