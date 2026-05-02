using System.ComponentModel.DataAnnotations;

namespace AnketPortal.API.DTOs
{
    public class SurveyCreateDto
    {
        [Required(ErrorMessage = "Anket başlığı zorunludur.")]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
    }
}