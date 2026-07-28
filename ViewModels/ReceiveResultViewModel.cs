using System.ComponentModel.DataAnnotations;

namespace MedCore.ViewModels
{
    public class ReceiveResultViewModel
    {
        public int LabOrderId { get; set; }

        [Required]
        public string Hl7ResultMessage { get; set; } = string.Empty;
    }
}