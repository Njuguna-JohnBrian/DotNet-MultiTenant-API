using Microsoft.EntityFrameworkCore;
using MultitenancyApp.DatabaseContext;
using MultitenancyApp.Helpers;
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
        IdentityDbContext identityContext,
        ApplicationDbContext applicationDbContext,
        IPasswordService passwordService)
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

        var tenantData = identityContext.Tenants.FirstOrDefault(tnt => tnt.Id.ToString() == tenantId);

        if (tenantData == null)
        {
            await _next(httpContext);
            return;
        }

        var tenantConnectionString = new NpgsqlConnectionStringBuilder
        {
            Host = ConfigurationHelper.GetConfigurationValueByKey("DbOptions:Host"),
            Port = int.Parse(ConfigurationHelper.GetConfigurationValueByKey("DbOptions:Port")!),
            Database = tenantData.TenantName,
            Username = tenantData.TenantName,
            Password = passwordService.DecryptPassword(tenantData.PasswordHash),
        }.ConnectionString;
        

        applicationDbContext.Database.SetConnectionString(tenantConnectionString);
        await applicationDbContext.Database.MigrateAsync();
        await _next.Invoke(httpContext);
    }
}