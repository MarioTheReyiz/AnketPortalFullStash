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

    public class UserListDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public IList<string> Roles { get; set; }
    }


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

        [HttpPost("Register")] 
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


                await _userManager.AddToRoleAsync(user, "User");

                return Ok(new ResultDto { Status = true, Message = "Kayıt Başarılı. Hesabınız standart 'User' yetkisiyle oluşturuldu." });
            }
            return BadRequest(new ResultDto { Status = false, Message = "Hata oluştu", Data = result.Errors });
        }

        [HttpPost("Login")] 
        public async Task<IActionResult> Login(LoginDto model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var roles = await _userManager.GetRolesAsync(user);

                var tokenResponse = _tokenService.GenerateToken(user, roles);

                user.RefreshToken = tokenResponse.RefreshToken;
                user.RefreshTokenEndDate = DateTime.Now.AddDays(7); 
                await _userManager.UpdateAsync(user);

                return Ok(new ResultDto { Status = true, Message = "Giriş Başarılı", Data = tokenResponse });
            }
            return Unauthorized(new ResultDto { Status = false, Message = "Kullanıcı adı veya şifre hatalı" });
        }

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

        [Authorize(Roles = "SuperAdmin")] 
        [HttpPost("AssignRole")]
        public async Task<IActionResult> AssignRole(RoleAssignDto model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null) return NotFound(new ResultDto { Status = false, Message = "Kullanıcı bulunamadı." });

            var currentRoles = await _userManager.GetRolesAsync(user);


            if (currentRoles.Contains("SuperAdmin"))
            {
                return BadRequest(new ResultDto { Status = false, Message = "SuperAdmin yetkisi değiştirilemez!" });
            }

            if (!await _roleManager.RoleExistsAsync(model.RoleName))
                return BadRequest(new ResultDto { Status = false, Message = "Böyle bir rol sistemde yok." });


            await _userManager.RemoveFromRolesAsync(user, currentRoles);


            var result = await _userManager.AddToRoleAsync(user, model.RoleName);

            if (result.Succeeded)
                return Ok(new ResultDto { Status = true, Message = $"{user.UserName} kullanıcısının yetkisi '{model.RoleName}' olarak değiştirildi." });

            return BadRequest(new ResultDto { Status = false, Message = "Yetki verilemedi.", Data = result.Errors });
        }

        [HttpPost("RefreshToken")] 
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto model)
        {

            if (string.IsNullOrEmpty(model?.RefreshToken))
                return BadRequest(new ResultDto { Status = false, Message = "Refresh Token boş olamaz." });


            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == model.RefreshToken);

            if (user == null || user.RefreshTokenEndDate < DateTime.Now)
                return Unauthorized(new ResultDto { Status = false, Message = "Geçersiz veya süresi dolmuş token. Lütfen tekrar giriş yapın." });

            var roles = await _userManager.GetRolesAsync(user);


            var tokenResponse = _tokenService.GenerateToken(user, roles);


            user.RefreshToken = tokenResponse.RefreshToken;
            user.RefreshTokenEndDate = DateTime.Now.AddDays(7);
            await _userManager.UpdateAsync(user);

            return Ok(new ResultDto { Status = true, Message = "Yenilendi", Data = tokenResponse });
        }

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


        [Authorize]
        [HttpGet("GetProfile")]
        public async Task<IActionResult> GetProfile()
        {

            var userName = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue("unique_name");
            var user = await _userManager.FindByNameAsync(userName ?? "");

            if (user == null)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
                user = await _userManager.FindByIdAsync(userId ?? "");
            }

            if (user == null) return NotFound(new ResultDto { Status = false, Message = "Kullanıcı bulunamadı, token geçersiz." });

            var roles = await _userManager.GetRolesAsync(user);

            var profile = new UserProfileDto
            {
                UserName = user.UserName,
                FullName = string.IsNullOrEmpty(user.FullName) ? user.UserName : user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfilePhoto = user.ProfilePhoto,
                Role = roles.FirstOrDefault() ?? "User"
            };

            return Ok(new ResultDto { Status = true, Data = profile });
        }

        [Authorize]
        [HttpPut("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto model)
        {
            var userName = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue("unique_name");
            var user = await _userManager.FindByNameAsync(userName ?? "");

            if (user == null)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
                user = await _userManager.FindByIdAsync(userId ?? "");
            }

            if (user == null) return NotFound(new ResultDto { Status = false, Message = "Kullanıcı bulunamadı" });

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;

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
        }        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            if (string.IsNullOrEmpty(model.Email))
                return BadRequest(new ResultDto { Status = false, Message = "E-posta adresi gereklidir." });

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Ok(new ResultDto { Status = true, Message = "Eğer sistemde kayıtlıysa, şifre sıfırlama bağlantısı e-posta adresinize gönderilmiştir." });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            return Ok(new ResultDto { Status = true, Message = "Sıfırlama bağlantısı e-posta adresinize gönderildi.", Data = token });
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Token) || string.IsNullOrEmpty(model.NewPassword))
                return BadRequest(new ResultDto { Status = false, Message = "Eksik bilgi." });

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BadRequest(new ResultDto { Status = false, Message = "Kullanıcı bulunamadı." });

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded)
                return Ok(new ResultDto { Status = true, Message = "Şifreniz başarıyla sıfırlandı. Giriş yapabilirsiniz." });

            return BadRequest(new ResultDto { Status = false, Message = string.Join(" ", result.Errors.Select(e => e.Description)) });
        }
    }
}