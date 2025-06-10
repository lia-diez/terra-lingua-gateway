using System.Text;
using System.Text.Json;

namespace Api.Services;

public class MapGenService
{
    private readonly HttpClient _httpClient;
    
    public MapGenService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        var baseAddress = configuration["MapGenService:url"] ?? throw new ArgumentNullException("MapGenService:url");
        _httpClient.BaseAddress = new Uri(baseAddress);
    }
    
    public async Task<byte[]> GenerateAsync(string commands)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "api/mapgen")
        {
            Content = new StringContent(JsonSerializer.Serialize(new { CommandsJson = commands }), Encoding.UTF8, "application/json")
        };

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsByteArrayAsync();
        return content;
    }
}