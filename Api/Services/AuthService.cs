using Api.DB;
using Api.DB.Models;
using Api.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class AuthService
{
    private readonly ILogger<AuthService> _logger;
    private readonly AppDbContext _db;
    private readonly PasswordHasher<object> _hasher = new();
    private readonly TokenService _tokenService;
    
    public AuthService(AppDbContext db, ILogger<AuthService> logger, TokenService tokenService)
    {
        _db = db;
        _logger = logger;
        _tokenService = tokenService;
    }
    
    public async Task<int> RegisterUserAsync(RegisterInDto dto)
    {
        if (dto == null || string.IsNullOrEmpty(dto.Username) || string.IsNullOrEmpty(dto.Password) || dto.Password != dto.PasswordConfirm)
        {
            _logger.LogWarning("Invalid registration data provided.");
            throw new ArgumentException("Invalid registration data.");
        }

        var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
        if (existingUser != null)
        {
            _logger.LogWarning("Username already exists: {Username}", dto.Username);
            throw new InvalidOperationException("Username already exists.");
        }

        var user = new User
        {
            Username = dto.Username,
            HashedPwd = _hasher.HashPassword(null, dto.Password),
        };

        return await AddUser(user);
    }

    private async Task<int> AddUser(User user)
    {
        _db.Users.Add(user);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user to the database");
            throw;
        }
        return user.UserId;
    }
    
    public async Task<User> LoginUserAsync(LoginInDto dto)
    {
        if (dto == null || string.IsNullOrEmpty(dto.Username) || string.IsNullOrEmpty(dto.Password))
        {
            _logger.LogWarning("Invalid login data provided.");
            throw new ArgumentException("Invalid login data.");
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
        if (user == null)
        {
            _logger.LogWarning("Login failed for non-existent user: {Username}", dto.Username);
            throw new InvalidOperationException("User does not exist.");
        }

        var result = _hasher.VerifyHashedPassword(null, user.HashedPwd, dto.Password);
        if (result != PasswordVerificationResult.Success)
        {
            _logger.LogWarning("Login failed for user: {Username}", dto.Username);
            throw new InvalidOperationException("Invalid password.");
        }

        return user;
    }

    public async Task<string> GenerateTokenAsync(User user)
    {
        if (user == null)
        {
            _logger.LogWarning("Cannot generate token for null user.");
            throw new ArgumentNullException(nameof(user), "User cannot be null.");
        }

        var token = await _tokenService.GenerateTokenAsync(user);
        
        _logger.LogInformation("Generated token for user: {Username}", user.Username);
        return token;
    }
}