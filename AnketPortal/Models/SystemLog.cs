namespace AnketPortal.API.Models
{
    public class SystemLog : BaseEntity
    {
        public string Action { get; set; } = string.Empty; 
        public string Message { get; set; } = string.Empty; 
        public string Username { get; set; } = string.Empty;
        public string Color { get; set; } = "bg-primary";
        public string Icon { get; set; } = "fa-info-circle"; 
    }
}