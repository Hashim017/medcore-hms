using Microsoft.AspNetCore.Identity;

namespace MedCore.Models
{
    // Extends Identity's built-in user so login/roles work automatically
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        public int? DoctorId { get; set; }
        public Doctor? Doctor { get; set; }
    }
}