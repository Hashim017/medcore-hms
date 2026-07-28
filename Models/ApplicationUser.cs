using Microsoft.AspNetCore.Identity;

namespace MedCore.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}