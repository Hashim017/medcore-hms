using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedCore.Models
{
    public enum PaymentStatus
    {
        Unpaid,
        Paid
    }

    public class Bill
    {
        public int Id { get; set; }

        public int AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Unpaid;

        public DateTime GeneratedOn { get; set; } = DateTime.UtcNow;
        public DateTime? PaidOn { get; set; }
    }
}