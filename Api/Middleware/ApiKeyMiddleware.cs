using Api.DB;
using Api.Middleware.Attributes;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Middleware;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private const string ApiKeyHeaderName = "X-API-KEY";
    private readonly ILogger<ApiKeyService> _logger;
    private readonly TokenService _tokenService;

    public ApiKeyMiddleware(RequestDelegate next, ILogger<ApiKeyService> logger, TokenService tokenService)
    {
        _next = next;
        _logger = logger;
        _tokenService = tokenService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var auth = endpoint?.Metadata.GetMetadata<AuthAttribute>();
        if (auth?.Type != AuthType.ApiKey)
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("API Key was not provided.");
            return;
        }

        var userId = await _tokenService.ValidateTokenAsync(extractedApiKey.ToString());
        if (userId == null)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Unauthorized client.");
            return;
        }
        context.Items["UserId"] = userId;

        await _next(context);
    }
}