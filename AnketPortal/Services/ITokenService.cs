using AnketPortal.API.Models;

namespace AnketPortal.API.Repositories
{
    public interface ITokenService
    {
        string GenerateToken(AppUser user, IList<string> roles);
    }
}