using System.ComponentModel.DataAnnotations;
using MedCore.Models;

namespace MedCore.ViewModels
{
    public class AppointmentViewModel
    {
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required, DataType(DataType.DateTime)]
        public DateTime AppointmentDate { get; set; }

        public AppointmentStatus Status { get; set; }

        public string? Notes { get; set; }
    }
}