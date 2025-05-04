using System;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using MyGallery.Api.Models;
using System.Diagnostics;
using System.Text;

namespace MyGallery.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : Controller
{
    private readonly IConfiguration _config;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IConfiguration config, ILogger<AuthController> logger)
    {
        _config = config;
        _logger = logger;
    }

    [HttpGet("/debug-vars")]
    public IActionResult DebugVars()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Environment Variables:");
        sb.AppendLine($"ADMIN_EMAIL: '{Environment.GetEnvironmentVariable("ADMIN_EMAIL")}'");
        sb.AppendLine($"ADMIN_PASSWORD length: {Environment.GetEnvironmentVariable("ADMIN_PASSWORD")?.Length ?? 0}");

        sb.AppendLine("\nConfig Values:");
        sb.AppendLine($"AdminCredentials:Email: '{_config["AdminCredentials:Email"]}'");
        sb.AppendLine($"AdminCredentials:Password length: {_config["AdminCredentials:Password"]?.Length ?? 0}");

        return Content(sb.ToString(), "text/plain");
    }

    [HttpPost("/Login")]
    public IActionResult Login([FromForm] LoginModel model)
    {
        try
        {
            _logger.LogInformation($"Login attempt: Email={model?.Email ?? "null"}");

            // Debug the form data
            foreach (var key in Request.Form.Keys)
            {
                _logger.LogInformation($"Form data: {key}={Request.Form[key]}");
            }

            if (model == null)
            {
                _logger.LogWarning("Login model is null");
                return RedirectToAction("LoginPage", new { error = "No login data provided" });
            }

            _logger.LogInformation($"Model data - Email: '{model.Email}', Password length: {(model.Password?.Length ?? 0)}");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state");
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        _logger.LogWarning($"Model error - {state.Key}: {error.ErrorMessage}");
                    }
                }

                // For API calls, maintain JSON response
                if (Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    return BadRequest(ModelState);
                }

                // For form submissions, redirect with error
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return RedirectToAction("LoginPage", new { error = string.Join(", ", errors) });
            }

            // Try environment variables first
            var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
            var adminPass = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

            _logger.LogInformation($"AdminEmail from env: '{adminEmail}'");
            _logger.LogInformation($"AdminPass from env length: {(adminPass?.Length ?? 0)}");

            // If environment variables are not set, fall back to config
            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPass))
            {
                _logger.LogInformation("Environment variables not set, falling back to configuration");
                adminEmail = _config["AdminCredentials:Email"];
                adminPass = _config["AdminCredentials:Password"];
                _logger.LogInformation($"AdminEmail from config: '{adminEmail}'");
                _logger.LogInformation($"AdminPass from config length: {(adminPass?.Length ?? 0)}");
            }

            // Do a character-by-character comparison for logging purposes
            if (model.Email != null && adminEmail != null)
            {
                var compareResult = new StringBuilder("Email comparison: ");
                for (int i = 0; i < Math.Max(model.Email.Length, adminEmail.Length); i++)
                {
                    char modelChar = i < model.Email.Length ? model.Email[i] : '⌀';
                    char adminChar = i < adminEmail.Length ? adminEmail[i] : '⌀';
                    compareResult.Append($"[{i}]{modelChar}=={adminChar}({modelChar == adminChar}) ");
                }
                _logger.LogInformation(compareResult.ToString());
            }

            if (model.Password != null && adminPass != null)
            {
                _logger.LogInformation($"Password length comparison: {model.Password.Length} vs {adminPass.Length}");
            }

            _logger.LogInformation($"Exact comparison: '{model.Email}' == '{adminEmail}' : {model.Email == adminEmail}");
            _logger.LogInformation($"Exact comparison: Password == AdminPass : {model.Password == adminPass}");

            // Try using hardcoded credentials temporarily for debugging
            if ((model.Email == "mbakry484@gmail.com" && model.Password == "Aabbcc_112233") ||
                (model.Email == adminEmail && model.Password == adminPass))
            {
                _logger.LogInformation("Login successful");
                HttpContext.Session.SetString("IsAdmin", "true");
                return RedirectToAction("Dashboard");
            }

            _logger.LogWarning($"Invalid credentials. Expected: '{adminEmail}', Got: '{model.Email}'");

            // For API calls, maintain JSON response
            if (Request.Headers["Accept"].ToString().Contains("application/json"))
            {
                return BadRequest("Invalid Credentials");
            }

            // For form submissions, redirect with error
            return RedirectToAction("LoginPage", new { error = "Invalid email or password" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");

            // For API calls, maintain JSON response
            if (Request.Headers["Accept"].ToString().Contains("application/json"))
            {
                return StatusCode(500, "An error occurred during login");
            }

            // For form submissions, redirect with error
            return RedirectToAction("LoginPage", new { error = $"An error occurred during login: {ex.Message}" });
        }
    }

    [HttpGet("/Admin/Dashboard")]
    public IActionResult Dashboard()
    {
        if (HttpContext.Session.GetString("IsAdmin") != "true")
        {
            return RedirectToAction("LoginPage");
        }

        return File("~/admin/gallery.html", "text/html");
    }

    [HttpGet("/Login")]
    public IActionResult LoginPage()
    {
        if (HttpContext.Session.GetString("IsAdmin") == "true")
        {
            return RedirectToAction("Dashboard");
        }

        return File("~/login.html", "text/html");
    }

    [HttpPost("/Logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove("IsAdmin");
        return RedirectToAction("LoginPage");
    }
}
