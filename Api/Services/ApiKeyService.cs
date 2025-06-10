using Api.DB;
using Api.DB.Models;
using Microsoft.EntityFrameworkCore;

public class ApiKeyService
{
    private readonly ILogger<ApiKeyService> _logger;
    private readonly AppDbContext _db;
    private readonly int _validSeconds;

    public ApiKeyService(ILogger<ApiKeyService> logger, AppDbContext db, IConfiguration configuration)
    {
        _logger = logger;
        _db = db;
        _validSeconds = int.Parse(configuration["ApiKey:ValidSeconds"] ?? "86400");
        
    }

    public  async Task<bool> IsValidApiKey(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("API key is null or empty.");
            return false;
        }

        var apiKeyRecord = _db.ApiKeys.AsQueryable().FirstOrDefault();
        if (apiKeyRecord == null)
        {
            _logger.LogWarning("Invalid API key: {ApiKey}", apiKey);
            return false;
        }

        if (apiKeyRecord.ValidTill < DateTime.UtcNow)
        {
            _logger.LogWarning("API key expired: {ApiKey}", apiKey);
            _db.ApiKeys.Remove(apiKeyRecord);
            await _db.SaveChangesAsync();
            return false;
        }
        return true;
    }

    public async Task<int> GenerateApiKeyAsync(int userId)
    {
        if (userId <= 0)
        {
            _logger.LogWarning("Cannot generate API key for null or empty user ID.");
            throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty.");
        }

        var apiKey = new ApiKey
        {
            UserId = userId,
            KeyString = Guid.NewGuid().ToString().Replace("-", ""),
            ValidTill = DateTime.UtcNow.AddSeconds(_validSeconds)
        };

        _db.ApiKeys.Add(apiKey);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Generated API key for user: {UserId}", userId);
        return apiKey.KeyId;
    }

    public async Task<IEnumerable<ApiKey>> GetApiKeysAsync(int userId)
    {
        if (userId <= 0)
        {
            _logger.LogWarning("Cannot retrieve API keys for null or empty user ID.");
            throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty.");
        }

        var apiKeys = await _db.ApiKeys.AsQueryable().Where(k => k.UserId == userId).ToListAsync();
        if ( apiKeys.Count == 0)
        {
            _logger.LogWarning("No API keys found for user: {UserId}", userId);
            return Enumerable.Empty<ApiKey>();
        }

        return apiKeys;
    }
    

    public async Task<bool> DeleteApiKeyAsync(int userId, int keyId)
    {
        if (userId <= 0 || keyId <= 0)
        {
            _logger.LogWarning("Cannot delete API key for null or empty user ID or key ID.");
            throw new ArgumentNullException("User ID and Key ID cannot be null or empty.");
        }

        var apiKey = await _db.ApiKeys.AsQueryable().FirstOrDefaultAsync(k => k.UserId == userId && k.KeyId == keyId);
        if (apiKey == null)
        {
            _logger.LogWarning("API key not found for user: {UserId}, Key ID: {KeyId}", userId, keyId);
            return false;
        }
        _db.ApiKeys.Remove(apiKey);
        try
        {
            await _db.SaveChangesAsync();
            _logger.LogInformation("Deleted API key for user: {UserId}, Key ID: {KeyId}", userId, keyId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting API key for user: {UserId}, Key ID: {KeyId}", userId, keyId);
            return false;
        }
    }
}