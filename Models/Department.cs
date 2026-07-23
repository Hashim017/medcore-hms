using System.ComponentModel.DataAnnotations;

namespace MedCore.Models
{
    public class Department
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty; // e.g. Cardiology, Pediatrics

        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}