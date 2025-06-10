using Api.Middleware.Attributes;

namespace Api.Middleware;

public class BearerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<BearerMiddleware> _logger;
    private readonly TokenService _tokenService;

    public BearerMiddleware(RequestDelegate next, TokenService tokenService, ILogger<BearerMiddleware> logger)
    {
        _next = next;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var auth = endpoint?.Metadata.GetMetadata<AuthAttribute>();
        if (auth?.Type != AuthType.Bearer)
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader) || !authHeader.ToString().StartsWith("Bearer "))
        {
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsync("Bearer token was not provided.");
            return;
        }

        var token = authHeader.ToString().Substring("Bearer ".Length).Trim();
        if (string.IsNullOrEmpty(token))
        {
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsync("Bearer token is empty.");
            return;
        }

        try
        {
            var userId = await _tokenService.ValidateTokenAsync(token);
            if (userId == null)
            {
                context.Response.StatusCode = 403; 
                await context.Response.WriteAsync("Invalid or expired token.");
                return;
            }
            context.Items["UserId"] = userId; // Store user ID in the context for later use
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 400; // Internal Server Error
            await context.Response.WriteAsync("Bad token.");
            return;
        }

        await _next(context);
    }
}