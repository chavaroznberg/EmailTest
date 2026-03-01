using EmailApi.Models;
using EmailApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmailApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailController : ControllerBase
{
    private readonly IRateLimitingService _rateLimitingService;
    private readonly ILogger<EmailController> _logger;

    public EmailController(IRateLimitingService rateLimitingService, ILogger<EmailController> logger)
    {
        _rateLimitingService = rateLimitingService;
        _logger = logger;
    }

    /// <summary>
    /// Accepts an email address and returns the time the server received it.
    /// Rate limiting is enforced via middleware (max 1 request every 3 seconds per email).
    /// </summary>
    [HttpPost]
    public IActionResult Post([FromBody] EmailInputModel model)
    {
        if (string.IsNullOrWhiteSpace(model?.Email))
        {
            _logger.LogWarning("Email validation failed - empty or null email provided");
            return BadRequest(new { message = "Email is required." });
        }

        var now = DateTime.UtcNow;
        var response = new EmailOutputModel 
        { 
            Email = model.Email, 
            ReceivedAt = now.ToString("o") 
        };

        _logger.LogInformation($"Email request processed: {model.Email}");
        return Ok(response);
    }
}
