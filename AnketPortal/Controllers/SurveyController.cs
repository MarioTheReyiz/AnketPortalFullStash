using AnketPortal.API.DTOs;
using AnketPortal.API.Models;
using AnketPortal.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace AnketPortal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SurveyController : ControllerBase
    {
        private readonly IGenericRepository<Survey> _surveyRepo;
        private readonly IGenericRepository<Question> _questionRepo;
        private readonly IGenericRepository<QuestionOption> _optionRepo;

        public SurveyController(
            IGenericRepository<Survey> surveyRepo,
            IGenericRepository<Question> questionRepo,
            IGenericRepository<QuestionOption> optionRepo)
        {
            _surveyRepo = surveyRepo;
            _questionRepo = questionRepo;
            _optionRepo = optionRepo;
        }

        //Aktif Anketleri Listeleme ve Arama
        [HttpGet]
        public async Task<IActionResult> GetSurveys(string? search = null)
        {
            var surveys = await _surveyRepo.GetAllAsync();
            var activeSurveys = surveys.Where(s => s.IsActive);

            // Eğer arama yapılmışsa başlıkta filtrele
            if (!string.IsNullOrEmpty(search))
            {
                activeSurveys = activeSurveys.Where(s => s.Title.ToLower().Contains(search.ToLower()));
            }

            var result = activeSurveys.Select(s => new SurveyDto
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                CreatedDate = s.CreatedDate
            });
            return Ok(result);
        }

        //Anket Detaylarını Getirme
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

        //  Yeni Anket Oluşturma
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
                IsActive = true
            };

            await _surveyRepo.AddAsync(survey);
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

            _surveyRepo.Update(survey);
            await _surveyRepo.SaveAsync();

            return Ok(new ResultDto { Status = true, Message = "Anket başarıyla güncellendi." });
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

        //  Soruya Şık  Ekleme
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost("AddOption")]
        public async Task<IActionResult> AddOption(OptionCreateDto model)
        {
            var option = new QuestionOption
            {
                OptionText = model.OptionText,
                Order = model.Order,
                QuestionId = model.QuestionId,
                CreatedDate = DateTime.Now
            };

            await _optionRepo.AddAsync(option);
            await _optionRepo.SaveAsync();

            return Ok(new ResultDto { Status = true, Message = "Şık başarıyla eklendi." });
        }

        // Anket Sonuçları
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpGet("{id}/Results")]
        public async Task<IActionResult> GetSurveyResults(int id)
        {
            return Ok(new ResultDto { Status = true, Message = $"{id} numaralı anketin sonuçları derleniyor. (İstatistikler Final projesinde aktif edilecek)" });
        }

        //  Anket Silme (Soft Delete)
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSurvey(int id)
        {
            var survey = await _surveyRepo.GetByIdAsync(id);
            if (survey == null) return NotFound();

            survey.IsActive = false;
            _surveyRepo.Update(survey);
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
    }
}