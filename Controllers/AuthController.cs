using Microsoft.EntityFrameworkCore;
using MultitenancyApp.DatabaseContext;
using MultitenancyApp.DatabaseContext.Models;
using MultitenancyApp.Interfaces;
using MultitenancyApp.Requests;

namespace MultitenancyApp.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IdentityDbContext _identityContext;
    private readonly ApplicationDbContext _applicationContext;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;
    private readonly ITenantService _tenantService;

    public AuthController(IdentityDbContext identityContext, IPasswordService passwordService, ITokenService tokenService,
        ITenantService tenantService, ApplicationDbContext applicationContext)
    {
        _identityContext = identityContext;
        _passwordService = passwordService;
        _tokenService = tokenService;
        _tenantService = tenantService;
        _applicationContext = applicationContext;
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> RegisterTenant([FromBody] AuthRequest request)
    {
        var tenant = new Tenant
        {
            Email = request.Email,
            PasswordHash = _passwordService.CreatePasswordHash(request.Password),
            TenantName = request.TenantName
        };


        if (_identityContext.Tenants.Any(u => u.Email == request.Email || u.TenantName == request.TenantName))
            return Conflict(new { message = "Tenant exists." });

        _identityContext.Tenants.Add(tenant);

        await _identityContext.SaveChangesAsync();

        await _tenantService.CreateTenantDatabaseAsync(tenant.TenantName, request.Password);


        return Ok(new { message = "Tenant registered successfully" });
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> LoginTenant([FromBody] LoginRequest request)
    {
        var tenantInfo = await _identityContext.Tenants.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (tenantInfo == null) return NotFound(new { message = $"Tenant with email {request.Email} not found." });

        if (!_passwordService.PasswordIsValid(request.Password, tenantInfo.PasswordHash))
        {
            return UnprocessableEntity(new { message = "Tenant password is invalid." });
        }

        string token = _tokenService.CreateToken(tenantInfo);

        var tenantInfoClone = new
        {
            tenantInfo.Id,
            tenantInfo.Email,
            tenantInfo.TenantName,
            tenantInfo.CreatedAt,
            ConnectionString = _applicationContext.Database.GetDbConnection().ConnectionString,
        };

        return Ok(new
        {
            message = "Tenant logged in successfully",
            tenant_info = tenantInfoClone,
            acces_token = token,
            type_type = "Bearer"
        });
    }
}