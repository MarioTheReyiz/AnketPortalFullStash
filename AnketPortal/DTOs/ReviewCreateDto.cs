namespace AnketPortal.API.DTOs
{
    public class ReviewCreateDto
    {
        public int StarCount { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}