using AnketPortal.API.DTOs;
using AnketPortal.API.Models;

namespace AnketPortal.API.Services
{
    public interface ITokenService
    {
        TokenResponseDto GenerateToken(AppUser user, IList<string> roles);
    }
}