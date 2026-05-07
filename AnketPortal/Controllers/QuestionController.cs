using AnketPortal.API.DTOs;
using AnketPortal.API.Models;
using AnketPortal.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AnketPortal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionController : ControllerBase
    {
        private readonly IGenericRepository<Question> _questionRepo;
        private readonly IGenericRepository<QuestionOption> _optionRepo;

        public QuestionController(IGenericRepository<Question> questionRepo, IGenericRepository<QuestionOption> optionRepo)
        {
            _questionRepo = questionRepo;
            _optionRepo = optionRepo;
        }

        [HttpGet("GetBySurvey/{surveyId}")]
        public async Task<IActionResult> GetBySurvey(int surveyId)
        {
            var questions = await _questionRepo.AsQueryable()
                .Include(q => q.Options.OrderBy(o => o.Order))
                .Where(q => q.SurveyId == surveyId)
                .Select(q => new
                {
                    q.Id,
                    q.Text,
                    q.MediaUrl,
                    q.Type,
                    q.IsRequired,
                    Options = q.Options.Select(o => new
                    {
                        o.Id,
                        o.OptionText,
                        o.ImageUrl,
                        o.Order,
                        o.NextQuestionId // GETİRİRKEN UI'A GÖNDERİYORUZ
                    })
                })
                .ToListAsync();

            return Ok(new ResultDto { Status = true, Data = questions });
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost("AddQuestion")]
        public async Task<IActionResult> AddQuestion(QuestionDto model)
        {
            var question = new Question
            {
                Text = model.Text,
                MediaUrl = model.MediaUrl,
                Type = (Models.Enums.QuestionType)model.Type,
                IsRequired = model.IsRequired,
                SurveyId = model.Id,
                CreatedDate = DateTime.Now
            };

            await _questionRepo.AddAsync(question);
            await _questionRepo.SaveAsync(); // Önce soruyu kaydet ki ID'si oluşsun

            if (model.Type != 1 && model.Options != null && model.Options.Any())
            {
                foreach (var opt in model.Options)
                {
                    var option = new QuestionOption
                    {
                        OptionText = opt.OptionText,
                        ImageUrl = opt.ImageUrl,
                        Order = opt.Order,
                        QuestionId = question.Id,
                        NextQuestionId = opt.NextQuestionId // KAYDEDERKEN VERİTABANINA YAZIYORUZ
                    };
                    await _optionRepo.AddAsync(option);
                }
                await _optionRepo.SaveAsync();
            }

            return Ok(new ResultDto { Status = true, Message = "Soru başarıyla eklendi." });
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPut("UpdateQuestion")]
        public async Task<IActionResult> UpdateQuestion(QuestionDto model)
        {
            var question = await _questionRepo.AsQueryable()
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == model.Id);

            if (question == null)
                return NotFound(new ResultDto { Status = false, Message = "Soru bulunamadı." });

            question.Text = model.Text;
            question.MediaUrl = model.MediaUrl;
            question.Type = (Models.Enums.QuestionType)model.Type;
            question.IsRequired = model.IsRequired;

            // Eski şıkları tamamen sil
            if (question.Options != null && question.Options.Any())
            {
                foreach (var oldOpt in question.Options.ToList())
                {
                    _optionRepo.Delete(oldOpt);
                }
            }

            // Yeni şıkları ekle
            if (model.Type != 1 && model.Options != null && model.Options.Any())
            {
                foreach (var opt in model.Options)
                {
                    var newOption = new QuestionOption
                    {
                        OptionText = opt.OptionText,
                        ImageUrl = opt.ImageUrl,
                        Order = opt.Order,
                        QuestionId = question.Id,
                        NextQuestionId = opt.NextQuestionId // EKSİK OLAN YER BURASIYDI, EKLENDİ!
                    };
                    await _optionRepo.AddAsync(newOption);
                }
            }

            _questionRepo.Update(question);
            await _questionRepo.SaveAsync();

            return Ok(new ResultDto { Status = true, Message = "Soru başarıyla güncellendi." });
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var question = await _questionRepo.GetByIdAsync(id);
            if (question == null)
                return NotFound(new ResultDto { Status = false, Message = "Soru bulunamadı." });

            _questionRepo.Delete(question);
            await _questionRepo.SaveAsync();

            return Ok(new ResultDto { Status = true, Message = "Soru başarıyla silindi." });
        }
    }
}