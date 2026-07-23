using System.ComponentModel.DataAnnotations;

namespace MedCore.ViewModels
{
    public class DoctorViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Specialization { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }

        [Required]
        public int DepartmentId { get; set; }
    }
}