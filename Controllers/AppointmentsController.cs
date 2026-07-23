using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MedCore.Data;
using MedCore.Models;
using MedCore.ViewModels;

namespace MedCore.Controllers
{
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AppointmentsController(ApplicationDbContext context) => _context = context;

        private async Task LoadDropdowns(int? patientId = null, int? doctorId = null)
        {
            ViewBag.Patients = new SelectList(await _context.Patients.ToListAsync(), "Id", "FullName", patientId);
            ViewBag.Doctors = new SelectList(await _context.Doctors.ToListAsync(), "Id", "FullName", doctorId);
        }

        public async Task<IActionResult> Index()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
            return View(appointments);
        }

        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(AppointmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns(model.PatientId, model.DoctorId);
                return View(model);
            }

            // Simple conflict check: same doctor, same exact time, not cancelled
            bool conflict = await _context.Appointments.AnyAsync(a =>
                a.DoctorId == model.DoctorId &&
                a.AppointmentDate == model.AppointmentDate &&
                a.Status != AppointmentStatus.Cancelled);

            if (conflict)
            {
                ModelState.AddModelError(string.Empty, "This doctor already has an appointment at that time.");
                await LoadDropdowns(model.PatientId, model.DoctorId);
                return View(model);
            }

            _context.Appointments.Add(new Appointment
            {
                PatientId = model.PatientId,
                DoctorId = model.DoctorId,
                AppointmentDate = model.AppointmentDate,
                Status = model.Status,
                Notes = model.Notes
            });
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var appt = await _context.Appointments.FindAsync(id);
            if (appt == null) return NotFound();

            await LoadDropdowns(appt.PatientId, appt.DoctorId);
            return View(new AppointmentViewModel
            {
                Id = appt.Id,
                PatientId = appt.PatientId,
                DoctorId = appt.DoctorId,
                AppointmentDate = appt.AppointmentDate,
                Status = appt.Status,
                Notes = appt.Notes
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, AppointmentViewModel model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid)
            {
                await LoadDropdowns(model.PatientId, model.DoctorId);
                return View(model);
            }

            var appt = await _context.Appointments.FindAsync(id);
            if (appt == null) return NotFound();

            appt.PatientId = model.PatientId;
            appt.DoctorId = model.DoctorId;
            appt.AppointmentDate = model.AppointmentDate;
            appt.Status = model.Status;
            appt.Notes = model.Notes;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var appt = await _context.Appointments
                .Include(a => a.Patient).Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (appt == null) return NotFound();
            return View(appt);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appt = await _context.Appointments.FindAsync(id);
            if (appt != null)
            {
                _context.Appointments.Remove(appt);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}