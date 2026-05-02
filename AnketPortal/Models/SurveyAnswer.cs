namespace AnketPortal.API.Models
{
    public class SurveyAnswer : BaseEntity
    {
        public string? TextAnswer { get; set; } 
        public int? SelectedOptionId { get; set; }
        public QuestionOption? SelectedOption { get; set; }



        public int QuestionId { get; set; }
        public Question Question { get; set; } = null!;


        public string AppUserId { get; set; } = string.Empty;
        public AppUser AppUser { get; set; } = null!; 
    }
}