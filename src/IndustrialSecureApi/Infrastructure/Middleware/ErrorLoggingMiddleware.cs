using System.Security.Cryptography;
using System.Text;

namespace IndustrialSecureApi.Infrastructure.Middleware;

public class ErrorLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorLoggingMiddleware> _logger;

    public ErrorLoggingMiddleware(RequestDelegate next, ILogger<ErrorLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Leggi il body della richiesta per calcolare l'hash
        string? bodyHash = null;
        
        // Abilita il buffering per permettere la lettura multipla del body
        context.Request.EnableBuffering();
        
        if (context.Request.Body.CanSeek && context.Request.Body.Length > 0)
        {
            var originalPosition = context.Request.Body.Position;
            context.Request.Body.Position = 0;

            using var memoryStream = new MemoryStream();
            await context.Request.Body.CopyToAsync(memoryStream);
            context.Request.Body.Position = originalPosition;

            var bodyBytes = memoryStream.ToArray();
            if (bodyBytes.Length > 0)
            {
                using var sha256 = SHA256.Create();
                var hashBytes = sha256.ComputeHash(bodyBytes);
                bodyHash = Convert.ToBase64String(hashBytes);
            }
        }

        // Esegui la richiesta
        await _next(context);

        // Logga solo se è un errore 4xx o 5xx
        var statusCode = context.Response.StatusCode;
        if (statusCode >= 400 && statusCode < 600)
        {
            var userId = context.User?.Identity?.Name ?? "Anonymous";
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var path = context.Request.Path;
            var method = context.Request.Method;

            _logger.LogWarning(
                "HTTP {StatusCode} {Method} {Path} - User: {UserId}, IP: {IpAddress}, BodyHash: {BodyHash}",
                statusCode,
                method,
                path,
                userId,
                ipAddress,
                bodyHash ?? "N/A"
            );
        }
    }
}

