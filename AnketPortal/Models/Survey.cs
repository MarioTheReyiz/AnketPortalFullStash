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

        public bool IsActive { get; set; } = true;
        public bool IsPublic { get; set; } = true; // true = Herkese Açık, false = Sadece Link
    }
}