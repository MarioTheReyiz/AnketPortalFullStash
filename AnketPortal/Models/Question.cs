using AnketPortal.API.Models.Enums;

namespace AnketPortal.API.Models
{
    public class Question : BaseEntity
    {
        public string Text { get; set; } = string.Empty;
        public QuestionType Type { get; set; } 
        public bool IsRequired { get; set; } = true; 


        public int SurveyId { get; set; }
        public Survey Survey { get; set; } = null!;

        public ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
    }
}