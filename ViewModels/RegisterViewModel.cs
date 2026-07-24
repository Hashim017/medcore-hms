using System.ComponentModel.DataAnnotations;

namespace MedCore.ViewModels
{
    public class RegisterViewModel
    {
        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;
        // Only required when Role == "Doctor"
        public string? Specialization { get; set; }
        public int? DepartmentId { get; set; }
        public string? PhoneNumber { get; set; }
    }
}