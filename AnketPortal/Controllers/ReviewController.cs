using AnketPortal.API.DTOs;
using AnketPortal.API.Models;
using AnketPortal.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AnketPortal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IGenericRepository<Review> _reviewRepo;

        public ReviewController(IGenericRepository<Review> reviewRepo)
        {
            _reviewRepo = reviewRepo;
        }

        // KULLANICI DEĞERLENDİRME GÖNDERİR
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddReview([FromBody] ReviewCreateDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var review = new Review
            {
                AppUserId = userId,
                StarCount = model.StarCount,
                Comment = model.Comment,
                CreatedDate = DateTime.Now
            };

            await _reviewRepo.AddAsync(review);
            await _reviewRepo.SaveAsync();

            return Ok(new ResultDto { Status = true, Message = "Değerlendirmeniz başarıyla alındı. Geri bildiriminiz için teşekkürler!" });
        }

        // ADMİN TÜM DEĞERLENDİRMELERİ LİSTELER
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetAllReviews()
        {
            var reviews = await _reviewRepo.AsQueryable()
                .Include(r => r.AppUser)
                .OrderByDescending(r => r.CreatedDate)
                .Select(r => new
                {
                    r.Id,
                    r.StarCount,
                    r.Comment,
                    Date = r.CreatedDate.ToString("dd.MM.yyyy HH:mm"),
                    UserName = r.AppUser.FullName ?? r.AppUser.UserName
                })
                .ToListAsync();

            return Ok(new ResultDto { Status = true, Data = reviews });
        }
    }
}