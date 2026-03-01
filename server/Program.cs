using EmailApi.Middleware;
using EmailApi.Services;

// create the WebApplication and configure DI services
var builder = WebApplication.CreateBuilder(args);

// configure logging
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});

// register MVC controllers
builder.Services.AddControllers();

// register services
builder.Services.AddSingleton<IRateLimitingService, RateLimitingService>();

// allow requests from the Angular development host (http://localhost:4200) and test server (http://localhost:3000)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// apply CORS policy FIRST before any other middleware
app.UseCors("AllowAngularDev");

// add Rate Limiting Middleware
app.UseMiddleware<RateLimitingMiddleware>();

app.MapControllers();

app.Run();
