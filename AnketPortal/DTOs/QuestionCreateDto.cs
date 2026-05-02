namespace AnketPortal.API.DTOs
{
    public class QuestionCreateDto
    {
        public int SurveyId { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Type { get; set; } // 1: Text, 2: MultipleChoice, 3: Checkbox
        public bool IsRequired { get; set; }

        // Çoktan seçmeli ise şıklar sadece metin (string) olarak gelecek
        public List<string> Options { get; set; } = new();
    }
}