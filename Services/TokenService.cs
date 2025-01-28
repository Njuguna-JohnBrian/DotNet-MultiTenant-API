using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using MultitenancyApp.DatabaseContext.Models;
using MultitenancyApp.Helpers;
using MultitenancyApp.Interfaces;

namespace MultitenancyApp.Services;

public class TokenService:ITokenService
{
    public string CreateToken(Tenant tenant)
    {
        if (tenant == null) throw new NullReferenceException();

        var claims = new List<Claim>()
        {
            new("Email", tenant.Email),
            new("TenantName", tenant.TenantName),
            new("TenantId", tenant.Id.ToString()),
        };

        var secret =
            new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(ConfigurationHelper.GetConfigurationValueByKey("JwtKey")!));
        var credentials = new SigningCredentials(secret, SecurityAlgorithms.HmacSha512Signature);
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public string DecodeTokenFromHeaders(HttpRequest httpRequest)
    {
        var bearerToken = httpRequest.Headers[HeaderNames.Authorization]
            .ToString().Replace("Bearer", "")
            .Trim();

        if (string.IsNullOrEmpty(bearerToken))
        {
            throw new InvalidOperationException("Failed to get  bearer token from request");
        }

        return bearerToken;
    }
}