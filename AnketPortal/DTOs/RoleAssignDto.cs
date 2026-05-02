namespace AnketPortal.API.DTOs
{
    public class RoleAssignDto
    {
        public string UserName { get; set; } = string.Empty; // Yetki verilecek kişi
        public string RoleName { get; set; } = string.Empty; // Verilecek rol 
    }
}