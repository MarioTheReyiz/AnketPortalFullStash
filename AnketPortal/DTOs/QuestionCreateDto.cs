namespace AnketPortal.API.DTOs
{
    public class QuestionCreateDto
    {
        public int SurveyId { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Type { get; set; } // 1: Text, 2: MultipleChoice, 3: Checkbox
        public bool IsRequired { get; set; }
        public string? MediaUrl { get; set; } // Soru Foto/Video
        public List<OptionCreateDto> Options { get; set; } = new();
    }

    public class OptionCreateDto
    {
        public string OptionText { get; set; } = string.Empty;
        public string? ImageUrl { get; set; } // Şık Foto
        public int Order { get; set; }      
        public int QuestionId { get; set; } 

        public int? NextQuestionId { get; set; }
    }
}