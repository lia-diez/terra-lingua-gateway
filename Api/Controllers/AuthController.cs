using Api.DB.Models;
using Api.DTOs;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : Controller
{
    private readonly ILogger<AuthController> _logger;
    private readonly AuthService _authService;
    
    public AuthController(ILogger<AuthController> logger, AuthService authService)
    {
        _logger = logger;
        _authService = authService;
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterInDto dto)
    {
        
        if (string.IsNullOrEmpty(dto.Username) || string.IsNullOrEmpty(dto.Password) || dto.Password != dto.PasswordConfirm)
        {
            _logger.LogWarning("Invalid registration data provided.");
            return BadRequest("Invalid registration data.");
        }
        try
        {
            var userId = await _authService.RegisterUserAsync(dto);
            _logger.LogInformation("User registered successfully with ID: {UserId}", userId);
            return Ok(new { UserId = userId });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid registration attempt for username: {Username}", dto.Username);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Registration failed for username: {Username}", dto.Username);
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during registration.");
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginInDto dto)
    {
        if (string.IsNullOrEmpty(dto.Username) || string.IsNullOrEmpty(dto.Password))
        {
            _logger.LogWarning("Invalid login data provided.");
            return BadRequest("Invalid login data.");
        }

        try
        {
            var user = await _authService.LoginUserAsync(dto);
            if (user == null)
            {
                _logger.LogWarning("Login failed for username: {Username}", dto.Username);
                return Unauthorized("Invalid username or password.");
            }

            _logger.LogInformation("User logged in successfully: {Username}", user.Username);
            var token = await _authService.GenerateTokenAsync(user);
            
            return Ok(new { UserId = user.UserId, token = token });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Login failed for username: {Username}", dto.Username);
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during login.");
            return StatusCode(500, "Internal server error");
        }
    }
    
    
}