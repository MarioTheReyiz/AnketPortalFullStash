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
                Type = (QuestionType)model.Type,
                IsRequired = model.IsRequired
            };

            if (model.Type != 1 && model.Options != null)
            {
                for (int i = 0; i < model.Options.Count; i++)
                {
                    question.Options.Add(new QuestionOption { OptionText = model.Options[i], Order = i + 1 });
                }
            }
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();
            return Ok(new ResultDto { Status = true, Message = "Soru kaydedildi." });
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
                    Options = q.Options.Select(o => new OptionDto { Id = o.Id, OptionText = o.OptionText, Order = o.Order }).OrderBy(o => o.Order).ToList()
                }).ToListAsync();
            return Ok(new ResultDto { Status = true, Data = questions });
        }
    }
}