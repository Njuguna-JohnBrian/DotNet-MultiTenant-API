using Microsoft.EntityFrameworkCore;
using MultitenancyApp.DatabaseContext;
using MultitenancyApp.Interfaces;
using Npgsql;

namespace MultitenancyApp.Middlewares;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext httpContext,
        IdentityDbContext _identityContext,
        ApplicationDbContext _applicationDbContext,
        IPasswordService _passwordService)
    {
        var apiPath = httpContext.Request.Path.Value?.ToLower();

        var ignoredPaths = new List<string>()
        {
            "/auth/register"
        };

        if (apiPath != null && ignoredPaths.Contains(apiPath))
        {
            await _next(httpContext);
            return;
        }


        var tenantId = httpContext.Request.Headers["TenantId"].ToString();

        var tenantData = _identityContext.Tenants.FirstOrDefault(tnt => tnt.Id.ToString() == tenantId);

        if (tenantData == null)
        {
            await _next(httpContext);
            return;
        }

        var tenantConnectionString = new NpgsqlConnectionStringBuilder
        {
            Host = "localhost",
            Port = 5432,
            Database = tenantData.TenantName,
            Username = tenantData.TenantName,
            Password = _passwordService.DecryptPassword(tenantData.PasswordHash),
        };

        Console.WriteLine(tenantConnectionString.ConnectionString);

        _applicationDbContext.Database.SetConnectionString(tenantConnectionString.ConnectionString);
        await _applicationDbContext.Database.MigrateAsync();
        await _next.Invoke(httpContext);
    }
}