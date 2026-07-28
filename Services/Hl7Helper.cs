using System.Text;
using MedCore.Models;

namespace MedCore.Services
{
    public static class Hl7Helper
    {
        // Builds a simplified ORM^O01 (Order Message) segment set
        public static string BuildOrderMessage(LabOrder order, Patient patient, Doctor doctor)
        {
            var ts = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var sb = new StringBuilder();

            sb.AppendLine($"MSH|^~\\&|MedCore|Hospital|LAB|LabSystem|{ts}||ORM^O01|{order.Id:D6}|P|2.3");
            sb.AppendLine($"PID|1||{patient.Id}^^^MedCore^MR||{patient.FullName}||{patient.DateOfBirth:yyyyMMdd}|{patient.Gender}");
            sb.AppendLine($"ORC|NW|{order.Id:D6}");
            sb.AppendLine($"OBR|1|{order.Id:D6}||{order.TestCode}^{order.TestName}^L|||{ts}|||{doctor.FullName}");

            return sb.ToString();
        }

        // Parses a simplified ORU^R01 (Result Message) — pulls the result value out of the OBX segment
        public static string? ParseResultValue(string hl7Message)
        {
            var lines = hl7Message.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var obxLine = lines.FirstOrDefault(l => l.StartsWith("OBX", StringComparison.OrdinalIgnoreCase));
            if (obxLine == null) return null;

            // OBX|1|TX|CODE^NAME||RESULT_VALUE|...
            var fields = obxLine.Split('|');
            return fields.Length > 5 ? fields[5] : null;
        }
    }
}