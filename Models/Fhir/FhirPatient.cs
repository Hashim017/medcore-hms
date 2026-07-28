namespace MedCore.Models.Fhir
{
    public class FhirPatient
    {
        public string ResourceType { get; set; } = "Patient";
        public string Id { get; set; } = string.Empty;
        public List<FhirName> Name { get; set; } = new();
        public string Gender { get; set; } = string.Empty;
        public string BirthDate { get; set; } = string.Empty;
        public List<FhirTelecom> Telecom { get; set; } = new();
        public List<FhirAddress> Address { get; set; } = new();
    }

    public class FhirName
    {
        public string Use { get; set; } = "official";
        public string Text { get; set; } = string.Empty;
    }

    public class FhirTelecom
    {
        public string System { get; set; } = string.Empty; // "phone" or "email"
        public string Value { get; set; } = string.Empty;
    }

    public class FhirAddress
    {
        public string Text { get; set; } = string.Empty;
    }
}