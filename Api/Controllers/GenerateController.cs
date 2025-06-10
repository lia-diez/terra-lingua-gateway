using Api.DTOs;
using Api.Middleware.Attributes;
using Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GenerateController
{
    private readonly LlmService _llm;
    private readonly MapGenService _mapGen;
    private readonly ILogger<GenerateController> _logger;

    public GenerateController(LlmService llm, MapGenService mapGen, ILogger<GenerateController> logger)
    {
        _llm = llm;
        _mapGen = mapGen;
        _logger = logger;
    }

    [HttpPost]
    [Auth(AuthType.ApiKey)]
    public async Task<IActionResult> Generate([FromBody] GenerateInputDto dto)
    {
        var result = await _llm.GenerateAsync(dto.Prompt);
        if (string.IsNullOrEmpty(result))
        {
            return new BadRequestObjectResult("Failed to generate response.");
        }

        var mapGenResult = await _mapGen.GenerateAsync(result);
        if (mapGenResult.Length == 0)
        {
            return new BadRequestObjectResult("Failed to generate map.");
        }

        _logger.LogInformation("Commands: {Commands}", result);
        var fileName = $"{Guid.NewGuid()}.png";
        var contentType = "image/png";

        return new FileContentResult(mapGenResult, contentType)
        {
            FileDownloadName = fileName
        };
    }
}