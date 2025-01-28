using MultitenancyApp.DatabaseContext.Models;

namespace MultitenancyApp.Interfaces;

public interface ITokenService
{
    string CreateToken(Tenant tenant);
    string DecodeTokenFromHeaders(HttpRequest httpRequest);
}