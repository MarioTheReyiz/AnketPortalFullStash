namespace AnketPortal.API.DTOs
{
    public class SurveyDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<QuestionDto> Questions { get; set; } = new();
    }
}