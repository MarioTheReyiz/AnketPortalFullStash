using AnketPortal.API.DTOs;
using AnketPortal.API.Models;
using AnketPortal.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AnketPortal.API.Controllers
{
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
                var token = _tokenService.GenerateToken(user, roles);
                return Ok(new ResultDto { Status = true, Message = "Giriş Başarılı", Data = token });
            }
            return Unauthorized(new ResultDto { Status = false, Message = "Kullanıcı adı veya şifre hatalı" });
        }

        
        [Authorize(Roles = "SuperAdmin")] // Yetki Atama API'si
        [HttpPost("AssignRole")]
        public async Task<IActionResult> AssignRole(RoleAssignDto model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null) return NotFound(new ResultDto { Status = false, Message = "Kullanıcı bulunamadı." });

            if (!await _roleManager.RoleExistsAsync(model.RoleName))
                return BadRequest(new ResultDto { Status = false, Message = "Böyle bir rol sistemde yok." });

            // Kullanıcıya yetkiyi ver
            var result = await _userManager.AddToRoleAsync(user, model.RoleName);

            if (result.Succeeded)
                return Ok(new ResultDto { Status = true, Message = $"{user.UserName} adlı kullanıcıya '{model.RoleName}' yetkisi verildi." });

            return BadRequest(new ResultDto { Status = false, Message = "Yetki verilemedi.", Data = result.Errors });
        }
    }
}