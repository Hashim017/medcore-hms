using System.ComponentModel.DataAnnotations;

namespace MedCore.Models
{
    public class Doctor
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Specialization { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }

        public int DepartmentId { get; set; }
        public Department? Department { get; set; }

        // Optional link to a login account (set only if this doctor can log in)
        public string? ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}