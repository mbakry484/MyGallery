using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace MyGallery.Api.Services
{
    public class ImgBBService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ImgBBService> _logger;

        public ImgBBService(HttpClient httpClient, IConfiguration configuration, ILogger<ImgBBService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _apiKey = configuration["ImgBB:ApiKey"] ?? "385eabbc2bcc9634f2e18edd6831df52"; // Fallback to provided key

            _logger.LogInformation($"ImgBB API Key configured: {_apiKey.Substring(0, 4)}...{_apiKey.Substring(_apiKey.Length - 4)}");
        }

        public async Task<string> UploadImageAsync(IFormFile imageFile)
        {
            try
            {
                _logger.LogInformation($"Starting upload to ImgBB: {imageFile.FileName}, size: {imageFile.Length} bytes");

                // Create a new form to send to ImgBB
                using var formData = new MultipartFormDataContent();

                // Convert the image file to bytes and add to form
                using var memoryStream = new MemoryStream();
                await imageFile.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();

                _logger.LogInformation($"Converted image to byte array, length: {imageBytes.Length}");

                var imageContent = new ByteArrayContent(imageBytes);
                formData.Add(imageContent, "image", imageFile.FileName);

                // According to the API docs, the key should be a URL parameter not a form field
                string url = $"https://api.imgbb.com/1/upload?key={_apiKey}";
                _logger.LogInformation($"Sending request to ImgBB API with URL: {url.Substring(0, url.IndexOf("?key=") + 5)}[API_KEY_MASKED]");

                // Send the request to ImgBB
                var response = await _httpClient.PostAsync(url, formData);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Response status: {response.StatusCode}");
                _logger.LogInformation($"Response content: {responseContent}");

                // Check if the request was successful
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"ImgBB API request failed: {response.StatusCode}, {responseContent}");
                    throw new Exception($"ImgBB API request failed: {response.StatusCode}, {responseContent}");
                }

                // Parse the response to get the image URL
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var imgBBResponse = JsonSerializer.Deserialize<ImgBBResponse>(responseContent, options);

                if (imgBBResponse?.Data?.Url == null)
                {
                    _logger.LogError($"ImgBB did not return a valid image URL. Response: {responseContent}");
                    throw new Exception("ImgBB did not return a valid image URL");
                }

                _logger.LogInformation($"Successfully uploaded to ImgBB. URL: {imgBBResponse.Data.Url}");

                // Return the direct image URL
                return imgBBResponse.Data.Url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image to ImgBB");
                throw new Exception($"Error uploading image to ImgBB: {ex.Message}", ex);
            }
        }

        // Classes to deserialize ImgBB response
        private class ImgBBResponse
        {
            public bool Success { get; set; }
            public ImgBBData Data { get; set; }
        }

        private class ImgBBData
        {
            public string Id { get; set; }
            public string Title { get; set; }
            [JsonPropertyName("url_viewer")]
            public string UrlViewer { get; set; }
            public string Url { get; set; }
            [JsonPropertyName("display_url")]
            public string DisplayUrl { get; set; }
            public int Size { get; set; }
            public long Time { get; set; }
            public long Expiration { get; set; }
            public ImgBBImage Image { get; set; }
            public ImgBBImage Thumb { get; set; }
            public ImgBBImage Medium { get; set; }
            [JsonPropertyName("delete_url")]
            public string DeleteUrl { get; set; }
        }

        private class ImgBBImage
        {
            public string Filename { get; set; }
            public string Name { get; set; }
            public string Mime { get; set; }
            public string Extension { get; set; }
            public string Url { get; set; }
        }
    }
}