using AnketPortal.API.Data;
using AnketPortal.API.DTOs;
using AnketPortal.API.Models;
using AnketPortal.API.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnketPortal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Sadece giriş yapan adminler soru ekleyebilir
    public class QuestionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public QuestionController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("AddQuestion")]
        public IActionResult AddQuestion(QuestionCreateDto model)
        {
            // 1. Soruyu Oluştur (Gelen int Type'ı Enum'a çeviriyoruz)
            var newQuestion = new Question
            {
                SurveyId = model.SurveyId,
                Text = model.Text,
                Type = (QuestionType)model.Type,
                IsRequired = model.IsRequired
            };

            // 2. Eğer soru metin değilse (Seçmeli veya Onay Kutusuysa) şıkları ekle
            if (model.Type != 1 && model.Options != null && model.Options.Count > 0)
            {
                for (int i = 0; i < model.Options.Count; i++)
                {
                    newQuestion.Options.Add(new QuestionOption
                    {
                        OptionText = model.Options[i],
                        Order = i + 1 // 1, 2, 3 diye sıralansın
                    });
                }
            }

            // 3. Veritabanına kaydet
            _context.Questions.Add(newQuestion);
            _context.SaveChanges();

            return Ok(new ResultDto { Status = true, Message = "Soru başarıyla eklendi." });
        }
        [HttpGet("GetQuestions/{surveyId}")]
        public IActionResult GetQuestions(int surveyId)
        {
            // İlgili anketin sorularını ve o sorulara ait şıkları veritabanından çekiyoruz
            var questions = _context.Questions
                .Where(q => q.SurveyId == surveyId)
                .Select(q => new
                {
                    q.Id,
                    q.Text,
                    q.Type,
                    q.IsRequired,
                    // Şıkları sırasına (Order) göre dizip alıyoruz
                    Options = q.Options.OrderBy(o => o.Order).Select(o => new
                    {
                        o.Id,
                        o.OptionText
                    }).ToList()
                })
                .ToList();

            return Ok(new ResultDto { Status = true, Message = "Sorular listelendi", Data = questions });
        }
    }
}