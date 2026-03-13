using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartFintechFinancial.Modules.Identity.Infrastructure.Persistence;

namespace SmartFintechFinancial.API.Controllers;

[Authorize] // All endpoints in this controller require authentication
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IdentityDbContext _context;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IdentityDbContext context, ILogger<UsersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                u.Phone,
                u.EmailVerified,
                u.KycStatus,
                u.RiskLevel,
                u.Currency,
                u.Language,
                u.CreatedAt,
                u.LastLoginAt,
                Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound(new { error = "User not found" });
        }

        return Ok(user);
    }

    [HttpGet("profile")]
    [Authorize(Policy = "VerifiedKycOnly")] // Only users with verified KYC
    public async Task<IActionResult> GetProfile()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                u.Phone,
                u.Address,
                u.City,
                u.Country,
                u.KycStatus,
                u.RiskLevel,
                u.DailyTransactionLimit,
                u.MonthlyTransactionLimit,
                u.InvestmentRiskTolerance,
                u.PrimaryInvestmentGoal,
                u.Currency,
                u.CreatedAt
            })
            .FirstOrDefaultAsync();

        return Ok(user);
    }

    [HttpGet("admin-only")]
    [Authorize(Policy = "AdminOnly")]
    public IActionResult AdminOnlyEndpoint()
    {
        return Ok(new { message = "You have admin access!" });
    }

    [HttpGet("advisor-only")]
    [Authorize(Policy = "AdvisorOnly")]
    public IActionResult AdvisorOnlyEndpoint()
    {
        return Ok(new { message = "You have advisor access!" });
    }
}