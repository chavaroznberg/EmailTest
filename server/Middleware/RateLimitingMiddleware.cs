using System.Text.Json;
using EmailApi.Models;
using EmailApi.Services;
using Microsoft.AspNetCore.Http;

namespace EmailApi.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IRateLimitingService rateLimitingService)
    {
        // Only check rate limiting for POST requests to the email endpoint
        if (context.Request.Method == "POST" && context.Request.Path.StartsWithSegments("/api/email"))
        {
            try
            {
                // Enable buffering so we can read the body multiple times
                context.Request.EnableBuffering();
                
                // Read the request body
                using (var reader = new StreamReader(context.Request.Body, leaveOpen: true))
                {
                    var body = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;

                    _logger.LogInformation($"Request body: {body}");

                    var emailModel = JsonSerializer.Deserialize<EmailInputModel>(body);
                    
                    _logger.LogInformation($"[MIDDLEWARE] Deserialized emailModel: {(emailModel?.Email ?? "NULL")}  | Email WhiteSpace Check: {string.IsNullOrWhiteSpace(emailModel?.Email)}");

                    if (!string.IsNullOrWhiteSpace(emailModel?.Email))
                    {
                        _logger.LogInformation($"[MIDDLEWARE] About to check rate limit for: {emailModel.Email}");
                        if (!rateLimitingService.IsAllowed(emailModel.Email, out var lastReceivedAt))
                        {
                            _logger.LogInformation($"[MIDDLEWARE] Rate limit check RETURNED FALSE - blocking request");
                            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                            context.Response.ContentType = "application/json";

                            var errorResponse = new RateLimitErrorModel
                            {
                                Email = emailModel.Email,
                                ReceivedAt = lastReceivedAt,
                                StatusCode = 429,
                                Message = "Too Many Requests - Please wait at least 3 seconds before resubmitting"
                            };

                            _logger.LogWarning($"Rate limit exceeded for email: {emailModel.Email}. Last request: {lastReceivedAt}");
                            await context.Response.WriteAsJsonAsync(errorResponse);
                            return;
                        }

                        _logger.LogInformation($"[MIDDLEWARE] Rate limit check RETURNED TRUE - allowing request");
                        _logger.LogInformation($"Request allowed for email: {emailModel.Email}");
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError($"JSON parsing error in middleware: {ex.Message}");
            }
        }

        await _next(context);
    }
}
