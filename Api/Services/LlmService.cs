using System.Text;
using System.Text.Json;

namespace Api.Services;

public class LlmService
{
    private readonly HttpClient _httpClient;
    
    public LlmService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        var baseAddress = configuration["LlmService:url"] ?? throw new ArgumentNullException("LlmService:url");
        _httpClient.BaseAddress = new Uri(baseAddress);
    }
    
    public async Task<string> GenerateAsync(string prompt)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "api/plan/generate")
        {
            Content = new StringContent(JsonSerializer.Serialize(new { prompt }), Encoding.UTF8, "application/json")
        };

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return content;
    }
}