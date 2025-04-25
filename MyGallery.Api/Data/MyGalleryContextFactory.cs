using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using MyGallery.Api.Data;
using System.IO;

namespace MyGallery.Api
{
    public class MyGalleryContextFactory : IDesignTimeDbContextFactory<MyGalleryContext>
    {
        public MyGalleryContext CreateDbContext(string[] args)
        {
            // Build configuration from appsettings.json
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("MyGallery");

            var optionsBuilder = new DbContextOptionsBuilder<MyGalleryContext>();
            optionsBuilder.UseSqlite(connectionString);

            return new MyGalleryContext(optionsBuilder.Options);
        }
    }
}