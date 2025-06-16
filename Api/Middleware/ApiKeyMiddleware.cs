using Api.DB;
using Api.Middleware.Attributes;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Middleware;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private const string ApiKeyHeaderName = "X-API-KEY";
    private readonly ILogger<ApiKeyService> _logger;
    private readonly ApiKeyService _apiKeyService;

    public ApiKeyMiddleware(RequestDelegate next, ILogger<ApiKeyService> logger, ApiKeyService apiKeyService)
    {
        _next = next;
        _logger = logger;
        _apiKeyService = apiKeyService;
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

        var isValid = await _apiKeyService.IsValidApiKey(extractedApiKey);
        if (!isValid)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Unauthorized client.");
            return;
        }

        await _next(context);
    }
}