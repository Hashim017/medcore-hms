using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedCore.Data;
using MedCore.Models.Fhir;

namespace MedCore.Controllers.Api
{
    [ApiController]
    [Route("fhir")]
    [Authorize]
    public class FhirController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public FhirController(ApplicationDbContext context) => _context = context;

        // GET /fhir/Patient/5
        [HttpGet("Patient/{id}")]
        public async Task<IActionResult> GetPatient(int id)
        {
            var p = await _context.Patients.FindAsync(id);
            if (p == null) return NotFound();

            var fhirPatient = new FhirPatient
            {
                Id = p.Id.ToString(),
                Name = new() { new FhirName { Text = p.FullName } },
                Gender = p.Gender,
                BirthDate = p.DateOfBirth.ToString("yyyy-MM-dd"),
                Telecom = new()
                {
                    new FhirTelecom { System = "phone", Value = p.PhoneNumber ?? "" },
                    new FhirTelecom { System = "email", Value = p.Email ?? "" }
                },
                Address = new() { new FhirAddress { Text = p.Address ?? "" } }
            };

            return Ok(fhirPatient);
        }

        // GET /fhir/Appointment/5
        [HttpGet("Appointment/{id}")]
        public async Task<IActionResult> GetAppointment(int id)
        {
            var a = await _context.Appointments
                .Include(x => x.Patient).Include(x => x.Doctor)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (a == null) return NotFound();

            var fhirAppointment = new FhirAppointment
            {
                Id = a.Id.ToString(),
                Status = a.Status.ToString().ToLower(),
                Start = a.AppointmentDate.ToString("o"),
                Participant = new()
                {
                    new FhirParticipant { Actor = new FhirReference { Reference = $"Patient/{a.PatientId}", Display = a.Patient?.FullName ?? "" } },
                    new FhirParticipant { Actor = new FhirReference { Reference = $"Practitioner/{a.DoctorId}", Display = a.Doctor?.FullName ?? "" } }
                }
            };

            return Ok(fhirAppointment);
        }
    }
}