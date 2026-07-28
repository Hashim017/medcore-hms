using System.ComponentModel.DataAnnotations;

namespace MedCore.ViewModels
{
    public class LabOrderViewModel
    {
        [Required]
        public int PatientId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required, StringLength(20)]
        public string TestCode { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string TestName { get; set; } = string.Empty;
    }
}