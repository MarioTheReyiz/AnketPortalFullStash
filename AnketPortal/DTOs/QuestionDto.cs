namespace AnketPortal.API.DTOs
{
    public class QuestionDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Type { get; set; } 
        public bool IsRequired { get; set; }
        public List<OptionDto> Options { get; set; } = new();
    }

    public class OptionDto
    {
        public int Id { get; set; }
        public string OptionText { get; set; } = string.Empty;
        public int Order { get; set; }
    }
}