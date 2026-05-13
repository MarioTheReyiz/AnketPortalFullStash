using AnketPortal.API.Data;
using AnketPortal.API.DTOs;
using AnketPortal.API.Models;
using AnketPortal.API.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AnketPortal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QuestionController : ControllerBase
    {
        private readonly AppDbContext _context;
        public QuestionController(AppDbContext context) => _context = context;

        [HttpPost("AddQuestion")]
        public async Task<IActionResult> AddQuestion(QuestionCreateDto model)
        {
            var question = new Question
            {
                SurveyId = model.SurveyId,
                Text = model.Text,
                Type = (AnketPortal.API.Models.Enums.QuestionType)model.Type,
                IsRequired = model.IsRequired,
                MediaUrl = model.MediaUrl
            };

            if (model.Type != 1 && model.Options != null)
            {
                foreach (var opt in model.Options)
                {
                    question.Options.Add(new QuestionOption
                    {
                        OptionText = opt.OptionText,
                        ImageUrl = opt.ImageUrl,
                        Order = question.Options.Count + 1,
                        NextQuestionId = opt.NextQuestionId
                    });
                }
            }

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();
            return Ok(new ResultDto { Status = true, Message = "Soru başarıyla kaydedildi." });
        }

        [HttpGet("GetBySurvey/{surveyId}")]
        public async Task<IActionResult> GetBySurvey(int surveyId)
        {
            var questions = await _context.Questions.Include(q => q.Options)
                .Where(q => q.SurveyId == surveyId)
                .Select(q => new QuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    Type = (int)q.Type,
                    IsRequired = q.IsRequired,
                    MediaUrl = q.MediaUrl,
                    Options = q.Options.Select(o => new OptionDto
                    {
                        Id = o.Id,
                        OptionText = o.OptionText,
                        ImageUrl = o.ImageUrl,
                        Order = o.Order,
                        NextQuestionId = o.NextQuestionId
                    }).OrderBy(o => o.Order).ToList()
                }).ToListAsync();
            return Ok(new ResultDto { Status = true, Data = questions });
        }

        // Soruyu ve bağlı olduğu şıkları siler
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var question = await _context.Questions.Include(q => q.Options).FirstOrDefaultAsync(x => x.Id == id);
            if (question == null) return NotFound(new ResultDto { Status = false, Message = "Soru bulunamadı." });

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
            return Ok(new ResultDto { Status = true, Message = "Soru başarıyla silindi." });
        }

        // Mevcut soruyu günceller
        [HttpPut("UpdateQuestion")]
        public async Task<IActionResult> UpdateQuestion(QuestionDto model)
        {
            var question = await _context.Questions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(x => x.Id == model.Id);

            if (question == null)
                return NotFound(new ResultDto { Status = false, Message = "Soru bulunamadı." });

            var optionIdsToDelete = question.Options.Select(o => o.Id).ToList();
            if (optionIdsToDelete.Any())
            {
                var relatedAnswers = await _context.SurveyAnswers
                    .Where(a => a.SelectedOptionId != null && optionIdsToDelete.Contains(a.SelectedOptionId.Value))
                    .ToListAsync();

                foreach (var ans in relatedAnswers)
                {
                    ans.SelectedOptionId = null;
                }
            }

            _context.QuestionOptions.RemoveRange(question.Options);

            if (model.Type != 1 && model.Options != null)
            {
                foreach (var opt in model.Options)
                {
                    question.Options.Add(new QuestionOption
                    {
                        OptionText = opt.OptionText,
                        ImageUrl = opt.ImageUrl,
                        Order = opt.Order,
                        NextQuestionId = opt.NextQuestionId
                    });
                }
            }

            question.Text = model.Text;
            question.IsRequired = model.IsRequired;
            question.Type = (QuestionType)model.Type;
            question.MediaUrl = model.MediaUrl;

            await _context.SaveChangesAsync();
            return Ok(new ResultDto { Status = true, Message = "Soru başarıyla güncellendi." });
        }
    }
}