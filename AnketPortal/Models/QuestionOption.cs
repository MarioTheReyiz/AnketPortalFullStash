namespace AnketPortal.API.Models
{
    public class QuestionOption : BaseEntity
    {
        public string OptionText { get; set; } = string.Empty;

        public string? ImageUrl { get; set; } // Şık fotoğrafı
        public int Order { get; set; }


        public int QuestionId { get; set; }
        public Question Question { get; set; } = null!;
    }
}