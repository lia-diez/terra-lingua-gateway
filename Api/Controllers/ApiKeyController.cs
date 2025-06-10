using Api.Middleware.Attributes;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApiKeyController : ControllerBase
{
    private readonly ApiKeyService _apiKeyService;
    public ApiKeyController(ApiKeyService apiKeyService)
    {
        _apiKeyService = apiKeyService;
    }

    [HttpPost("create")]
    [Auth(AuthType.Bearer)]
    public async Task<IActionResult> CreateApiKey()
    {
        int userId;
        Int32.TryParse(HttpContext.Items["UserId"] as string, out userId);
        await _apiKeyService.GenerateApiKeyAsync(userId);
        return Created();
    }
    
    [HttpGet("list")]
    [Auth(AuthType.Bearer)]
    public async Task<IActionResult> ListApiKeys()
    {
        int userId;
        Int32.TryParse(HttpContext.Items["UserId"] as string, out userId);
        var keys = await _apiKeyService.GetApiKeysAsync(userId);
        return Ok(keys);
    }
    
    [HttpDelete("delete/{keyId}")]
    [Auth(AuthType.Bearer)]
    public async Task<IActionResult> DeleteApiKey(int keyId)
    {
        int userId;
        Int32.TryParse(HttpContext.Items["UserId"] as string, out userId);
        var success = await _apiKeyService.DeleteApiKeyAsync(userId, keyId);
        if (success)
        {
            return NoContent();
        }
        return NotFound("API Key not found or does not belong to the user.");
    }
}