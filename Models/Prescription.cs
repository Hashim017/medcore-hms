using System.ComponentModel.DataAnnotations;

namespace MedCore.Models
{
    public class Prescription
    {
        public int Id { get; set; }

        public int AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }

        [Required]
        public string Medicines { get; set; } = string.Empty;

        public string? Instructions { get; set; }

        public DateTime IssuedOn { get; set; } = DateTime.UtcNow;
    }
}