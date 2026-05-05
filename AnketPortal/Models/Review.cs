using System;

namespace AnketPortal.API.Models
{
    public class Review : BaseEntity
    {
        public string AppUserId { get; set; } = string.Empty;
        public AppUser AppUser { get; set; } = null!;

        public int StarCount { get; set; } // 1 ile 5 arası yıldız
        public string Comment { get; set; } = string.Empty; // Kullanıcının yorumu
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}