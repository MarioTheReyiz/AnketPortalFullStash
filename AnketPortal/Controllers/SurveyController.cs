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

        // Aktif Anketleri Listeleme ve Arama (GÜNCELLENDİ: Yönetici her şeyi görebilir ve eksik veriler eklendi)
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

        // Yeni Anket Oluşturma (GÜNCELLENDİ: IsActive ve IsPublic eklendi)
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
            await _surveyRepo.SaveAsync();

            return Ok(new ResultDto { Status = true, Message = "Anket başarıyla oluşturuldu." });
        }

        // Anket Güncelleme (GÜNCELLENDİ: EndDate ve IsPublic eklendi)
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
            await _surveyRepo.SaveAsync();

            return Ok(new ResultDto { Status = true, Message = "Anket başarıyla güncellendi." });
        }

        // --- YENİ: Anlık Durum Değiştirme (Switch Butonu İçin) ---
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPut("ToggleActive/{id}")]
        public async Task<IActionResult> ToggleActiveStatus(int id)
        {
            var survey = await _surveyRepo.GetByIdAsync(id);
            if (survey == null) return NotFound(new ResultDto { Status = false, Message = "Anket bulunamadı." });

            // Durumu tersine çevir (True ise False, False ise True yap)
            survey.IsActive = !survey.IsActive;

            _surveyRepo.Update(survey);
            await _surveyRepo.SaveAsync();

            return Ok(new ResultDto { Status = true, Message = survey.IsActive ? "Anket Aktif edildi." : "Anket Pasife alındı." });
        }

        // Ankete Soru Ekleme (ORİJİNAL HALİYLE DURUYOR)
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

        // Soruya Şık Ekleme (ORİJİNAL HALİYLE DURUYOR)
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

        // Anket Sonuçları (ORİJİNAL HALİYLE DURUYOR)
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpGet("{id}/Results")]
        public async Task<IActionResult> GetSurveyResults(int id)
        {
            return Ok(new ResultDto { Status = true, Message = $"{id} numaralı anketin sonuçları derleniyor. (İstatistikler Final projesinde aktif edilecek)" });
        }

        // Anket Silme (Soft Delete) (ORİJİNAL HALİYLE DURUYOR)
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

        // Veritabanından fiziksel olarak sil (Hard Delete) (ORİJİNAL HALİYLE DURUYOR)
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