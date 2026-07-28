namespace MedCore.Models
{
    public enum LabOrderStatus { Ordered, ResultReceived }

    public class LabOrder
    {
        public int Id { get; set; }

        public int PatientId { get; set; }
        public Patient? Patient { get; set; }

        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; }

        public string TestCode { get; set; } = string.Empty;
        public string TestName { get; set; } = string.Empty;

        public LabOrderStatus Status { get; set; } = LabOrderStatus.Ordered;

        public DateTime OrderedOn { get; set; } = DateTime.UtcNow;
        public DateTime? ResultReceivedOn { get; set; }

        public string? ResultValue { get; set; }

        public string Hl7OrderMessage { get; set; } = string.Empty;
        public string? Hl7ResultMessage { get; set; }
    }
}