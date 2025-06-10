using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Api.DB.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

public class TokenService
{
    private readonly ILogger<TokenService> _logger;
    private readonly IConfiguration _config;
    
    private readonly string _jwtKey;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;
    private readonly int _jwtExpirationMinutes;

    public TokenService(ILogger<TokenService> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
        _jwtKey = _config["Jwt:Key"];
        _jwtIssuer = _config["Jwt:Issuer"];
        _jwtAudience = _config["Jwt:Audience"];
        _jwtExpirationMinutes = int.TryParse(_config["Jwt:ExpirationMinutes"], out var hours) ? hours : 1;
    }

    public async Task<string> GenerateTokenAsync(User user)
    {
        if (user == null)
        {
            _logger.LogWarning("Cannot generate token for null user.");
            throw new ArgumentNullException(nameof(user), "User cannot be null.");
        }
        
        var expires = DateTime.UtcNow.AddMinutes(_jwtExpirationMinutes);

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Name, user.Username),
        };

        var token = new JwtSecurityToken(
            issuer: _jwtIssuer,
            audience: _jwtAudience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        _logger.LogInformation("Generated JWT token for user: {Username}", user.Username);
        return await Task.FromResult(tokenString);
    }

    public async Task<string?> ValidateTokenAsync(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Token validation failed: token is null or empty.");
            throw new ArgumentException("Token cannot be null or empty.", nameof(token));
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = _jwtIssuer,
            ValidateAudience = true,
            ValidAudience = _jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero // No clock skew for simplicity
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            _logger.LogInformation("Token validated successfully for user: {Username}", principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name)?.Value);
            return await Task.FromResult(principal.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value);
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "Token validation failed.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during token validation.");
            throw new InvalidOperationException("Token validation failed.", ex);
        }
    }
    
}