using System.Text.Json.Serialization;

namespace EmailApi.Models;

public class EmailInputModel
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
}

public class EmailOutputModel
{
    public string Email { get; set; } = string.Empty;
    public string ReceivedAt { get; set; } = string.Empty;
}

public class RateLimitErrorModel
{
    public string Email { get; set; } = string.Empty;
    public string ReceivedAt { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string Message { get; set; } = "Too Many Requests";
}
