using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;

namespace MyGallery.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactController : ControllerBase
    {
        [HttpPost]
        public IActionResult SendContact([FromBody] ContactFormModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Message))
            {
                return BadRequest(new { error = "Email and message are required." });
            }

            try
            {
                // Configure your SMTP client here
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("mbakry484@gmail.com", "fdcn hjvz yeso owmm"),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(model.Email),
                    Subject = "New Contact Form Submission",
                    Body = $"From: {model.Email}\n\nMessage:\n{model.Message}",
                    IsBodyHtml = false,
                };
                mailMessage.To.Add("mbakry484@gmail.com");

                smtpClient.Send(mailMessage);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to send email.", details = ex.Message });
            }
        }

        public class ContactFormModel
        {
            public string Email { get; set; }
            public string Message { get; set; }
        }
    }
}