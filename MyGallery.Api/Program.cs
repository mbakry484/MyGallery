using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using MyGallery.Api.Data;
using Microsoft.AspNetCore.Http.Features;
using MyGallery.Api.Services;

var builder = WebApplication.CreateBuilder(args);
var connString = builder.Configuration.GetConnectionString("MyGallery");
builder.Services.AddDbContext<MyGalleryContext>(options =>
    options.UseSqlServer(connString));

// Register HttpClient and ImgBB service
builder.Services.AddHttpClient();
builder.Services.AddScoped<ImgBBService>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();

// Configure file upload settings
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10485760; // 10MB limit
});

// Add CORS support
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MyGallery API",
        Version = "v1",
        Description = "An API for managing gallery items"
    });

    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

//Admin Login
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession();
var app = builder.Build();

// Apply migrations and seed data at startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MyGalleryContext>();
    dbContext.Database.Migrate(); // This applies any pending migrations
}

// Enable CORS
app.UseCors();

app.UseSession();



//5
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyGallery API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

// Ensure wwwroot and uploads directories exist
var wwwrootPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
var uploadsPath = Path.Combine(wwwrootPath, "uploads");
if (!Directory.Exists(wwwrootPath))
{
    Directory.CreateDirectory(wwwrootPath);
}
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

// Enable serving static files
app.UseDefaultFiles();
app.UseStaticFiles();

// SPA fallback: serve index.html for unknown routes (except API and static files)
app.Use(async (context, next) =>
{
    await next();

    if (context.Response.StatusCode == 404 &&
        !System.IO.Path.HasExtension(context.Request.Path.Value) &&
        !context.Request.Path.Value.StartsWith("/api"))
    {
        context.Request.Path = "/index.html";
        context.Response.StatusCode = 200;
        await next();
    }
});

// Add MVC routing
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    // Redirect /index.html to /
    endpoints.MapGet("/index.html", context =>
    {
        context.Response.Redirect("/");
        return Task.CompletedTask;
    });


});

app.Run();



