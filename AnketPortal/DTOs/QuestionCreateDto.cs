namespace AnketPortal.API.DTOs
{
    public class QuestionCreateDto
    {
        public int SurveyId { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Type { get; set; } // 1: Metin, 2: Seçmeli, 3: Checkbox
        public bool IsRequired { get; set; }
        public List<string> Options { get; set; } = new(); // Şıkların sadece metinleri
    }
}