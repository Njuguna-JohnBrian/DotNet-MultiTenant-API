using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MultitenancyApp.DatabaseContext;
using MultitenancyApp.Helpers;
using MultitenancyApp.Interfaces;
using MultitenancyApp.Middlewares;
using MultitenancyApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<IdentityDbContext>(options =>
{
    options.UseNpgsql(ConfigurationHelper.GetConfigurationValueByKey("ConnectionStrings:IdentityConnection")!);
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(ConfigurationHelper.GetConfigurationValueByKey("ConnectionStrings:ApplicationConnection")!);
});

builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddCors();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddResponseCompression();
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddTransient<DbSeeder>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITenantService,TenantService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(ConfigurationHelper.GetConfigurationValueByKey("JwtKey")!)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

var app = builder.Build();

app.UseMiddleware<TenantMiddleware>();


app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(crs => crs.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseAuthorization();

app.MapControllers();

using var seederScope = app.Services.CreateScope();
try
{
    var seeder = seederScope.ServiceProvider.GetRequiredService<DbSeeder>();
    await seeder.SeedAsync();
}
catch (Exception ex)
{
    var logger = seederScope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred seeding the DB.");
}

app.Run();