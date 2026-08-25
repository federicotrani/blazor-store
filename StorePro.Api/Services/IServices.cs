using System.Security.Claims;

namespace StorePro.Api.Services;

public interface IPasswordService
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(Entities.User user);
    int? GetUserId(ClaimsPrincipal principal);
}
