namespace AnketPortal.API.Models
{
    public class QuestionOption : BaseEntity
    {
        public string OptionText { get; set; } = string.Empty;


        public int Order { get; set; }


        public int QuestionId { get; set; }
        public Question Question { get; set; } = null!;
    }
}