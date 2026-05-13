using AnketPortal.API.DTOs;
using AnketPortal.API.Models;
using AnketPortal.API.Services;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AnketPortal.API.Repositories
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        public TokenService(IConfiguration configuration) => _configuration = configuration;

        public TokenResponseDto GenerateToken(AppUser user, IList<string> roles)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("FullName", user.FullName ?? "")
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["DurationInMinutes"]));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiration,
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            var refreshToken = GenerateRefreshToken();

            return new TokenResponseDto
            {
                AccessToken = tokenHandler.WriteToken(token),
                RefreshToken = refreshToken,
                Expiration = expiration
            };
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}