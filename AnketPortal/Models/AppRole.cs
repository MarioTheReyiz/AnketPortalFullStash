using Microsoft.AspNetCore.Identity;

namespace AnketPortal.API.Models
{
    public class AppRole : IdentityRole
    {
        public string? Description { get; set; }
    }
}