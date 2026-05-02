namespace AnketPortal.API.DTOs
{
    public class ResultDto
    {
        public bool Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
    }
}