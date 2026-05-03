namespace AnketPortal.API.Models
{
    public class SystemLog : BaseEntity
    {
        public string Action { get; set; } = string.Empty; // Örn: "Anket Eklendi", "Kullanıcı Silindi"
        public string Message { get; set; } = string.Empty; // Örn: "Ahmet 'Müşteri Anketi'ni oluşturdu."
        public string Username { get; set; } = string.Empty; // İşlemi yapan kişi
        public string Color { get; set; } = "bg-primary"; // Logun rengi (bg-success, bg-danger vb.)
        public string Icon { get; set; } = "fa-info-circle"; // Logun ikonu
    }
}