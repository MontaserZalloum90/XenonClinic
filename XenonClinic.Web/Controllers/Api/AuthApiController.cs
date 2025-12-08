using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Services;
using XenonClinic.Web.Middleware;

namespace XenonClinic.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class AuthApiController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly ICompanyContext _companyContext;
    private readonly IBranchScopedService _branchScope;
    private readonly IAuditService _auditService;
    private readonly ILogger<AuthApiController> _logger;

    public AuthApiController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        ICompanyContext companyContext,
        IBranchScopedService branchScope,
        IAuditService auditService,
        ILogger<AuthApiController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _companyContext = companyContext;
        _branchScope = branchScope;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// User login - Returns JWT token for React SPA
    /// Rate limited to prevent brute force attacks
    /// </summary>
    [HttpPost("login")]
    [EnableRateLimiting(RateLimitingConfiguration.AuthPolicy)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null)
        {
            _logger.LogWarning("Login failed: User not found - {Username}", request.Username);
            await _auditService.LogAsync(new AuditLog
            {
                Action = AuditActions.LoginFailed,
                Description = $"Login failed: User not found - {request.Username}",
                IsSuccess = false,
                ErrorMessage = "User not found"
            });
            return Unauthorized(new { message = "Invalid username or password" });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            _logger.LogWarning("Login failed: Invalid password for user {Username}", request.Username);
            await _auditService.LogAsync(new AuditLog
            {
                Action = AuditActions.LoginFailed,
                UserId = user.Id,
                UserName = user.UserName,
                UserEmail = user.Email,
                Description = $"Login failed: Invalid password for user {request.Username}",
                IsSuccess = false,
                ErrorMessage = result.IsLockedOut ? "Account locked out" : "Invalid password"
            });
            return Unauthorized(new { message = "Invalid username or password" });
        }

        // Get user roles
        var roles = await _userManager.GetRolesAsync(user);

        // Generate JWT token
        var token = GenerateJwtToken(user, roles);

        _logger.LogInformation("User {Username} logged in successfully", request.Username);
        await _auditService.LogAsync(new AuditLog
        {
            Action = AuditActions.Login,
            UserId = user.Id,
            UserName = user.UserName,
            UserEmail = user.Email,
            Description = $"User {request.Username} logged in successfully",
            IsSuccess = true
        });

        return Ok(new LoginResponse
        {
            Token = token,
            User = new UserInfo
            {
                Id = user.Id,
                Username = user.UserName ?? "",
                Email = user.Email ?? "",
                FullName = user.FullName ?? "",
                Roles = roles.ToList()
            }
        });
    }

    /// <summary>
    /// User registration
    /// Rate limited to prevent abuse
    /// </summary>
    [HttpPost("register")]
    [EnableRateLimiting(RateLimitingConfiguration.AuthPolicy)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = new ApplicationUser
        {
            UserName = request.Username,
            Email = request.Email,
            FullName = request.FullName,
            EmailConfirmed = true // Auto-confirm for demo
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            _logger.LogWarning("Registration failed for {Username}: {Errors}",
                request.Username,
                string.Join(", ", result.Errors.Select(e => e.Description)));
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        // Add default role
        await _userManager.AddToRoleAsync(user, "User");

        _logger.LogInformation("New user registered: {Username}", request.Username);
        await _auditService.LogAsync(new AuditLog
        {
            Action = AuditActions.Create,
            EntityType = "User",
            EntityId = user.Id,
            UserId = user.Id,
            UserName = user.UserName,
            UserEmail = user.Email,
            Description = $"New user registered: {request.Username}",
            IsSuccess = true
        });

        return Ok(new { message = "Registration successful. Please login." });
    }

    /// <summary>
    /// Get current user info
    /// </summary>
    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound();

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new UserInfo
        {
            Id = user.Id,
            Username = user.UserName ?? "",
            Email = user.Email ?? "",
            FullName = user.FullName ?? "",
            Roles = roles.ToList()
        });
    }

    /// <summary>
    /// Refresh token endpoint
    /// </summary>
    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        var roles = await _userManager.GetRolesAsync(user);
        var token = GenerateJwtToken(user, roles);

        return Ok(new { token });
    }

    private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? "XenonClinic-SecureKey-12345678901234567890123456789012";
        var issuer = jwtSettings["Issuer"] ?? "XenonClinic";
        var audience = jwtSettings["Audience"] ?? "XenonClinicReact";
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "1440");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? ""),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim("FullName", user.FullName ?? "")
        };

        // Add roles as claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

// DTOs for API requests/responses
public class LoginRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

public class RegisterRequest
{
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Password { get; set; } = "";
}

public class LoginResponse
{
    public string Token { get; set; } = "";
    public UserInfo User { get; set; } = new();
}

public class UserInfo
{
    public string Id { get; set; } = "";
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string FullName { get; set; } = "";
    public List<string> Roles { get; set; } = new();
}
