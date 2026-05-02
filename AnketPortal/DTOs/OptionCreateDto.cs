namespace AnketPortal.API.DTOs
{
    public class OptionCreateDto
    {
        public int QuestionId { get; set; } 
        public string OptionText { get; set; } = string.Empty; 
        public int Order { get; set; } 
    }
}   