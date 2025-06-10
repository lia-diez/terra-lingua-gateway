using Api.Middleware.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]

public class Health : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return Ok(new
        {
            Status = "Healthy",
            Time = DateTime.UtcNow.ToString("o")
        });
    }
}