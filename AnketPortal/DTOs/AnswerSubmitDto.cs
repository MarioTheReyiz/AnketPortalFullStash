using System.ComponentModel.DataAnnotations;

namespace AnketPortal.API.DTOs
{
    public class AnswerSubmitDto
    {
        [Required(ErrorMessage = "Hangi ankete cevap verdiğinizi (SurveyId) belirtmek zorundasınız.")]
        public int SurveyId { get; set; }

        [Required(ErrorMessage = "Cevaplar listesi boş olamaz.")]
        public List<QuestionAnswerDto> Answers { get; set; } = new();
    }

    public class QuestionAnswerDto
    {
        [Required(ErrorMessage = "Hangi soruya (QuestionId) cevap verdiğinizi belirtmek zorundasınız.")]
        public int QuestionId { get; set; }

        public string? TextAnswer { get; set; } 

        public int? SelectedOptionId { get; set; } //
    }
}