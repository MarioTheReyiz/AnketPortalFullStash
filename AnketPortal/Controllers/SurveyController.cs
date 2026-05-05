using AnketPortal.API.DTOs;
using AnketPortal.API.Models;
using AnketPortal.API.Repositories;
using AnketPortal.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AnketPortal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SurveyController : ControllerBase
    {
        private readonly IGenericRepository<Survey> _surveyRepo;
        private readonly IGenericRepository<Question> _questionRepo;
        private readonly IGenericRepository<QuestionOption> _optionRepo;
        private readonly AppDbContext _context;

        public SurveyController(
            IGenericRepository<Survey> surveyRepo,
            IGenericRepository<Question> questionRepo,
            IGenericRepository<QuestionOption> optionRepo,
            AppDbContext context)
        {
            _surveyRepo = surveyRepo;
            _questionRepo = questionRepo;
            _optionRepo = optionRepo;
            _context = context;
        }

        // Aktif Anketleri Listeleme ve Arama 
        [HttpGet]
        public async Task<IActionResult> GetSurveys(string? search = null)
        {
            var surveys = await _surveyRepo.GetAllAsync();
            var query = surveys.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s => s.Title.ToLower().Contains(search.ToLower()));
            }

            var result = query.OrderByDescending(s => s.Id).Select(s => new SurveyDto
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                CreatedDate = s.CreatedDate,
                EndDate = s.EndDate,
                IsActive = s.IsActive,
                IsPublic = s.IsPublic
            });
            return Ok(result);
        }

        // Anket Detaylarını Getirme
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSurveyById(int id)
        {
            var survey = await _surveyRepo.AsQueryable()
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (survey == null || !survey.IsActive)
                return NotFound(new ResultDto { Status = false, Message = "Anket bulunamadı." });

            return Ok(survey);
        }

        // Yeni Anket Oluşturma
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> CreateSurvey(SurveyCreateDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var survey = new Survey
            {
                Title = model.Title,
                Description = model.Description,
                EndDate = model.EndDate,
                AppUserId = userId!,
                CreatedDate = DateTime.Now,
                IsActive = model.IsActive,
                IsPublic = model.IsPublic
            };

            await _surveyRepo.AddAsync(survey);
            var log = new SystemLog
            {
                Action = "Yeni Anket",
                Message = $"'{survey.Title}' adlı anket oluşturuldu.",
                Username = User.Identity?.Name ?? "Sistem",
                Color = "bg-success",
                Icon = "fa-plus-circle",
                CreatedDate = DateTime.Now
            };
            _context.SystemLogs.Add(log);
            await _context.SaveChangesAsync();
            await _surveyRepo.SaveAsync();

            return Ok(new ResultDto { Status = true, Message = "Anket başarıyla oluşturuldu." });
        }

        // Anket Güncelleme 
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPut]
        public async Task<IActionResult> UpdateSurvey(SurveyDto model)
        {
            var survey = await _surveyRepo.GetByIdAsync(model.Id);
            if (survey == null) return NotFound(new ResultDto { Status = false, Message = "Anket bulunamadı." });

            survey.Title = model.Title;
            survey.Description = model.Description;
            survey.EndDate = model.EndDate;
            survey.IsPublic = model.IsPublic;

            _surveyRepo.Update(survey);
            var log = new SystemLog
            {
                Action = "Anket Güncellendi",
                Message = $"'{survey.Title}' adlı anket güncellendi.",
                Username = User.Identity?.Name ?? "Sistem",
                Color = "bg-info",
                Icon = "fa-edit",
                CreatedDate = DateTime.Now
            };
            _context.SystemLogs.Add(log);
            await _context.SaveChangesAsync();
            await _surveyRepo.SaveAsync();

            return Ok(new ResultDto { Status = true, Message = "Anket başarıyla güncellendi." });
        }

        // Anlık Durum Değiştirme (Switch Butonu İçin) 
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPut("ToggleActive/{id}")]
        public async Task<IActionResult> ToggleActiveStatus(int id)
        {
            var survey = await _surveyRepo.GetByIdAsync(id);
            if (survey == null) return NotFound(new ResultDto { Status = false, Message = "Anket bulunamadı." });

            survey.IsActive = !survey.IsActive;

            _surveyRepo.Update(survey);
            await _surveyRepo.SaveAsync();

            return Ok(new ResultDto { Status = true, Message = survey.IsActive ? "Anket Aktif edildi." : "Anket Pasife alındı." });
        }

        // Ankete Soru Ekleme
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost("AddQuestion")]
        public async Task<IActionResult> AddQuestion(QuestionDto model)
        {
            var question = new Question
            {
                Text = model.Text,
                Type = (Models.Enums.QuestionType)model.Type,
                IsRequired = model.IsRequired,
                SurveyId = model.Id,
                CreatedDate = DateTime.Now
            };

            await _questionRepo.AddAsync(question);
            await _questionRepo.SaveAsync();

            return Ok(new ResultDto { Status = true, Message = "Soru başarıyla eklendi." });
        }

        // Soruya Şık Ekleme 
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost("AddOption")]
        public async Task<IActionResult> AddOption(OptionCreateDto model)
        {
            var option = new QuestionOption
            {
                OptionText = model.OptionText,
                Order = model.Order,
                QuestionId = model.QuestionId,
                ImageUrl = model.ImageUrl,
                CreatedDate = DateTime.Now
            };

            await _optionRepo.AddAsync(option);
            await _optionRepo.SaveAsync();

            return Ok(new ResultDto { Status = true, Message = "Şık başarıyla eklendi." });
        }

        // Anket Silme (Soft Delete)
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSurvey(int id)
        {
            var survey = await _surveyRepo.GetByIdAsync(id);
            if (survey == null) return NotFound();

            survey.IsActive = false;
            _surveyRepo.Update(survey);
            var log = new SystemLog
            {
                Action = "Anket Silindi",
                Message = $"'{survey.Title}' adlı anket silindi (pasife alındı).",
                Username = User.Identity?.Name ?? "Sistem",
                Color = "bg-danger",
                Icon = "fa-trash",
                CreatedDate = DateTime.Now
            };
            _context.SystemLogs.Add(log);
            await _context.SaveChangesAsync();
            await _surveyRepo.SaveAsync();

            return Ok(new ResultDto { Status = true, Message = "Anket başarıyla silindi (pasife alındı)." });
        }

        // Veritabanından fiziksel olarak sil (Hard Delete)
        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("{id}/HardDelete")]
        public async Task<IActionResult> HardDeleteSurvey(int id)
        {
            var survey = await _surveyRepo.GetByIdAsync(id);
            if (survey == null)
                return NotFound(new ResultDto { Status = false, Message = "Silinecek anket bulunamadı." });

            _surveyRepo.Delete(survey);
            await _surveyRepo.SaveAsync();

            return Ok(new ResultDto { Status = true, Message = "Anket ve ona bağlı tüm veriler veritabanından kalıcı olarak silindi!" });
        }

        [HttpGet("GetDashboardStats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var totalSurveys = await _context.Surveys.CountAsync();
            var activeSurveys = await _context.Surveys.CountAsync(s => s.IsActive);
            var totalQuestions = await _context.Questions.CountAsync();
            var totalAnswers = await _context.SurveyAnswers.CountAsync();

            var stats = new
            {
                TotalSurveys = totalSurveys,
                ActiveSurveys = activeSurveys,
                TotalQuestions = totalQuestions,
                TotalAnswers = totalAnswers
            };

            return Ok(new ResultDto { Status = true, Data = stats });
        }

        // DİNAMİK BİLDİRİMLER
        [Authorize]
        [HttpGet("GetNotifications")]
        public async Task<IActionResult> GetNotifications()
        {
            var notifications = new List<object>();

            var lastSurvey = await _context.Surveys.OrderByDescending(s => s.Id).FirstOrDefaultAsync();
            if (lastSurvey != null)
            {
                notifications.Add(new
                {
                    Icon = "fa-clipboard-check",
                    Color = "bg-success",
                    Time = lastSurvey.CreatedDate.ToString("dd.MM.yyyy HH:mm"),
                    Message = $"Sisteme '{lastSurvey.Title}' adlı yeni bir anket eklendi."
                });
            }

            var passiveCount = await _context.Surveys.CountAsync(s => !s.IsActive);
            if (passiveCount > 0)
            {
                notifications.Add(new
                {
                    Icon = "fa-triangle-exclamation",
                    Color = "bg-warning",
                    Time = "Sistem Uyarısı",
                    Message = $"Şu an yayında olmayan {passiveCount} adet pasif anketiniz bulunuyor."
                });
            }

            var totalAnswers = await _context.SurveyAnswers.CountAsync();
            if (totalAnswers > 0)
            {
                notifications.Add(new
                {
                    Icon = "fa-chart-line",
                    Color = "bg-info",
                    Time = "İstatistik",
                    Message = $"Harika! Anketleriniz bugüne kadar toplam {totalAnswers} kez yanıtlandı."
                });
            }

            return Ok(new ResultDto { Status = true, Data = notifications });
        }
        // ANKET SONUÇLARINI / İSTATİSTİKLERİNİ GETİREN METOT
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpGet("{id}/Results")]
        public async Task<IActionResult> GetSurveyResults(int id)
        {
            var survey = await _surveyRepo.AsQueryable()
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (survey == null)
                return NotFound(new ResultDto { Status = false, Message = "Anket bulunamadı." });

            var questionIds = survey.Questions.Select(q => q.Id).ToList();

            // Bu ankete ait tüm cevapları al
            var allAnswers = await _context.SurveyAnswers
                .Where(a => questionIds.Contains(a.QuestionId))
                .ToListAsync();

            var resultData = new
            {
                Id = survey.Id,
                Title = survey.Title,
                Description = survey.Description,
                Questions = survey.Questions.Select(q => new
                {
                    Id = q.Id,
                    QuestionText = q.Text,
                    Type = q.Type,
                    // Bu soruya toplam kaç kişi cevap vermiş?
                    TotalVotes = allAnswers.Count(a => a.QuestionId == q.Id),

                    // Şıklı Sorular İçin (Radio/Checkbox) Oyları Say
                    Options = q.Options.Select(o => new
                    {
                        Id = o.Id,
                        // DÜZELTİLEN YER BURASI! (Sadece OptionText kullanıyoruz, olmayan Text'i sildik)
                        OptionText = o.OptionText,
                        Count = allAnswers.Count(a => a.QuestionId == q.Id && a.SelectedOptionId == o.Id)
                    }).ToList(),

                    // AÇIK UÇLU SORULAR İÇİN: Yazılan metinleri liste halinde al!
                    TextAnswers = allAnswers
                        .Where(a => a.QuestionId == q.Id && !string.IsNullOrWhiteSpace(a.TextAnswer))
                        .Select(a => a.TextAnswer)
                        .ToList()
                }).ToList()
            };

            return Ok(new ResultDto { Status = true, Data = resultData });
        }
        // ANKETİ ÇÖZEN KATILIMCILARIN LİSTESİ
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpGet("{id}/Participants")]
        public async Task<IActionResult> GetParticipants(int id)
        {
            // Bu ankete cevap vermiş benzersiz kullanıcıları bul
            var participants = await _context.SurveyAnswers
                .Where(a => a.Question.SurveyId == id)
                .Select(a => a.AppUser)
                .Distinct()
                .Select(u => new {
                    u.Id,
                    UserName = u.UserName,
                    FullName = u.FullName,
                    Email = u.Email
                })
                .ToListAsync();

            return Ok(new ResultDto { Status = true, Data = participants });
        }

        // BELİRLİ BİR KATILIMCININ CEVAP KAĞIDI
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpGet("{surveyId}/ParticipantDetails/{userId}")]
        public async Task<IActionResult> GetParticipantDetails(int surveyId, string userId)
        {
            var answers = await _context.SurveyAnswers
                .Include(a => a.Question)
                .Include(a => a.SelectedOption)
                .Where(a => a.Question.SurveyId == surveyId && a.AppUserId == userId)
                .Select(a => new {
                    QuestionText = a.Question.Text,
                    QuestionType = a.Question.Type,
                    AnswerText = a.TextAnswer,
                    SelectedOption = a.SelectedOption != null ? a.SelectedOption.OptionText : null
                })
                .ToListAsync();

            return Ok(new ResultDto { Status = true, Data = answers });
        }
    }
}