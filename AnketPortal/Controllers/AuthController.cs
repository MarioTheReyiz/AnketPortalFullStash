using AnketPortal.API.DTOs;
using AnketPortal.API.Models;
using AnketPortal.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AnketPortal.API.Controllers
{
    // Frontend'e listeleme için yollayacağımız DTO
    public class UserListDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public IList<string> Roles { get; set; }
    }

    // Refresh Token için DTO
    public class RefreshTokenRequestDto
    {
        public string RefreshToken { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly ITokenService _tokenService;

        public AuthController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
        }

        [HttpPost("Register")]  // Kayıt API'si
        public async Task<IActionResult> Register(RegisterDto model)
        {
            var user = new AppUser { UserName = model.UserName, Email = model.Email, FullName = model.FullName };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                string[] roles = { "SuperAdmin", "Admin", "User" };
                foreach (var role in roles)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        await _roleManager.CreateAsync(new AppRole { Name = role });
                    }
                }

                // İlk Kayıt Herkes User Olarak Başlar
                await _userManager.AddToRoleAsync(user, "User");

                return Ok(new ResultDto { Status = true, Message = "Kayıt Başarılı. Hesabınız standart 'User' yetkisiyle oluşturuldu." });
            }
            return BadRequest(new ResultDto { Status = false, Message = "Hata oluştu", Data = result.Errors });
        }

        [HttpPost("Login")] // Giriş API'si
        public async Task<IActionResult> Login(LoginDto model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var roles = await _userManager.GetRolesAsync(user);

                // Jeton paketini oluştur (AccessToken + RefreshToken + Expiration)
                var tokenResponse = _tokenService.GenerateToken(user, roles);

                // --- ÖNEMLİ: Refresh Token bilgilerini veritabanına işle ---
                user.RefreshToken = tokenResponse.RefreshToken;
                user.RefreshTokenEndDate = DateTime.Now.AddDays(7); // 7 gün boyunca şifresiz yenileyebilir
                await _userManager.UpdateAsync(user);

                return Ok(new ResultDto { Status = true, Message = "Giriş Başarılı", Data = tokenResponse });
            }
            return Unauthorized(new ResultDto { Status = false, Message = "Kullanıcı adı veya şifre hatalı" });
        }

        // --- YENİ: KULLANICILARI LİSTELEME ---
        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("GetUsers")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userList = new List<UserListDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new UserListDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    FullName = user.FullName,
                    Email = user.Email,
                    Roles = roles
                });
            }

            return Ok(new ResultDto { Status = true, Data = userList });
        }

        // --- GÜNCELLENEN: YETKİ ATAMA (Eskisini sil, yenisini ver) ---
        [Authorize(Roles = "SuperAdmin")] // Sadece SuperAdminler yetki değiştirebilir
        [HttpPost("AssignRole")]
        public async Task<IActionResult> AssignRole(RoleAssignDto model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null) return NotFound(new ResultDto { Status = false, Message = "Kullanıcı bulunamadı." });

            var currentRoles = await _userManager.GetRolesAsync(user);

            // GÜVENLİK: SuperAdmin kendi veya başka bir SuperAdmin'in yetkisini asla DÜŞÜREMEZ.
            if (currentRoles.Contains("SuperAdmin"))
            {
                return BadRequest(new ResultDto { Status = false, Message = "SuperAdmin yetkisi değiştirilemez!" });
            }

            if (!await _roleManager.RoleExistsAsync(model.RoleName))
                return BadRequest(new ResultDto { Status = false, Message = "Böyle bir rol sistemde yok." });

            // Önce kullanıcının sahip olduğu tüm eski rolleri sil (User ve Admin çakışmasın diye)
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            // Sonra yeni gelen tek rolü (User veya Admin) ata
            var result = await _userManager.AddToRoleAsync(user, model.RoleName);

            if (result.Succeeded)
                return Ok(new ResultDto { Status = true, Message = $"{user.UserName} kullanıcısının yetkisi '{model.RoleName}' olarak değiştirildi." });

            return BadRequest(new ResultDto { Status = false, Message = "Yetki verilemedi.", Data = result.Errors });
        }

        [HttpPost("RefreshToken")] // Jeton Yenileme API'si
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto model)
        {
            // model null ise veya içi boşsa
            if (string.IsNullOrEmpty(model?.RefreshToken))
                return BadRequest(new ResultDto { Status = false, Message = "Refresh Token boş olamaz." });

            // Veritabanında bu token'a sahip kullanıcıyı bul
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == model.RefreshToken);

            if (user == null || user.RefreshTokenEndDate < DateTime.Now)
                return Unauthorized(new ResultDto { Status = false, Message = "Geçersiz veya süresi dolmuş token. Lütfen tekrar giriş yapın." });

            var roles = await _userManager.GetRolesAsync(user);

            // Yeni token üret
            var tokenResponse = _tokenService.GenerateToken(user, roles);

            // Veritabanını güncelle
            user.RefreshToken = tokenResponse.RefreshToken;
            user.RefreshTokenEndDate = DateTime.Now.AddDays(7);
            await _userManager.UpdateAsync(user);

            return Ok(new ResultDto { Status = true, Message = "Yenilendi", Data = tokenResponse });
        }
        // --- PROFIL İÇİN DTO'LAR ---
        public class UserProfileDto
        {
            public string UserName { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public string ProfilePhoto { get; set; }
            public string Role { get; set; }
        }

        public class UpdateProfileDto
        {
            public string FullName { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public string ProfilePhoto { get; set; }
        }

        // --- YENİ: KULLANICI PROFİLİNİ GETİR ---
        [Authorize]
        [HttpGet("GetProfile")]
        public async Task<IActionResult> GetProfile()
        {
            // Token'dan giriş yapan kişinin ID'sini alıyoruz
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null) return NotFound(new ResultDto { Status = false, Message = "Kullanıcı bulunamadı" });

            var roles = await _userManager.GetRolesAsync(user);

            var profile = new UserProfileDto
            {
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfilePhoto = user.ProfilePhoto,
                Role = roles.FirstOrDefault() ?? "User"
            };

            return Ok(new ResultDto { Status = true, Data = profile });
        }

        // --- YENİ: KULLANICI PROFİLİNİ GÜNCELLE ---
        [Authorize]
        [HttpPut("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null) return NotFound(new ResultDto { Status = false, Message = "Kullanıcı bulunamadı" });

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            // Eğer yeni bir fotoğraf seçilmişse/gönderilmişse güncelle
            if (!string.IsNullOrEmpty(model.ProfilePhoto))
            {
                user.ProfilePhoto = model.ProfilePhoto;
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Ok(new ResultDto { Status = true, Message = "Profil başarıyla güncellendi!" });
            }

            return BadRequest(new ResultDto { Status = false, Message = "Profil güncellenirken bir hata oluştu." });
        }
    }
}