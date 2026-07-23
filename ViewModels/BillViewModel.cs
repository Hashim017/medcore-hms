using System.ComponentModel.DataAnnotations;
using MedCore.Models;

namespace MedCore.ViewModels
{
    public class BillViewModel
    {
        public int Id { get; set; }

        [Required]
        public int AppointmentId { get; set; }

        [Required, Range(0, 999999)]
        public decimal Amount { get; set; }

        public PaymentStatus Status { get; set; }
    }
}