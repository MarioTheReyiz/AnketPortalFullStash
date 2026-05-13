namespace AnketPortal.API.DTOs
{
    public class SurveyDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } 
        public bool IsPublic { get; set; } 
        public int QuestionCount { get; set; }
    }
}