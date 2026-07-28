namespace MedCore.Models.Fhir
{
    public class FhirAppointment
    {
        public string ResourceType { get; set; } = "Appointment";
        public string Id { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Start { get; set; } = string.Empty;
        public List<FhirParticipant> Participant { get; set; } = new();
    }

    public class FhirParticipant
    {
        public FhirReference Actor { get; set; } = new();
    }

    public class FhirReference
    {
        public string Reference { get; set; } = string.Empty; // e.g. "Patient/5"
        public string Display { get; set; } = string.Empty;
    }
}