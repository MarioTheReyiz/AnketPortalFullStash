namespace AnketPortal.API.Models
{
    public class Survey : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime EndDate { get; set; } 
        public string AppUserId { get; set; } = string.Empty;
        public AppUser AppUser { get; set; } = null!; 
        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public ICollection<SurveyAnswer> Answers { get; set; } = new List<SurveyAnswer>();
    }
}