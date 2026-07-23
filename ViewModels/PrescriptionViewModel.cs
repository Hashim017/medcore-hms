using System.ComponentModel.DataAnnotations;

namespace MedCore.ViewModels
{
    public class PrescriptionViewModel
    {
        public int Id { get; set; }

        [Required]
        public int AppointmentId { get; set; }

        [Required]
        public string Medicines { get; set; } = string.Empty;

        public string? Instructions { get; set; }
    }
}