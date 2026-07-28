using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MedCore.Data;
using MedCore.Models;
using MedCore.ViewModels;

namespace MedCore.Controllers
{
    [Authorize]
    public class PrescriptionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager; // ADDED

        public PrescriptionsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager) // ADDED param
        {
            _context = context;
            _userManager = userManager; // ADDED
        }

        private async Task LoadAppointments(int? selectedId = null)
        {
            var query = _context.Appointments
                .Include(a => a.Patient).Include(a => a.Doctor)
                .Where(a => a.Prescription == null)
                .AsQueryable();

            // ADDED: Doctor only sees their own appointments in the dropdown
            if (User.IsInRole("Doctor"))
            {
                var userId = _userManager.GetUserId(User);
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (doctor != null)
                    query = query.Where(a => a.DoctorId == doctor.Id);
            }

            var appointments = await query.ToListAsync();
            ViewBag.Appointments = new SelectList(
                appointments.Select(a => new { a.Id, Label = $"{a.Patient!.FullName} - {a.Doctor!.FullName} ({a.AppointmentDate:g})" }),
                "Id", "Label", selectedId);
        }

        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            int pageSize = 10;
            var query = _context.Prescriptions
                .Include(p => p.Appointment).ThenInclude(a => a!.Patient)
                .Include(p => p.Appointment).ThenInclude(a => a!.Doctor)
                .OrderByDescending(p => p.IssuedOn)
                .AsQueryable();

            // ADDED: Doctor only sees their own prescriptions
            if (User.IsInRole("Doctor"))
            {
                var userId = _userManager.GetUserId(User);
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (doctor != null)
                    query = query.Where(p => p.Appointment!.DoctorId == doctor.Id);
            }

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Appointment!.Patient!.FullName.Contains(search));

            int totalCount = await query.CountAsync();
            var prescriptions = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            ViewData["Search"] = search;
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalCount / (double)pageSize);
            return View(prescriptions);
        }

        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Create()
        {
            await LoadAppointments();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Create(PrescriptionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadAppointments(model.AppointmentId);
                return View(model);
            }
            _context.Prescriptions.Add(new Prescription
            {
                AppointmentId = model.AppointmentId,
                Medicines = model.Medicines,
                Instructions = model.Instructions
            });
            var appt = await _context.Appointments.FindAsync(model.AppointmentId);
            if (appt != null) appt.Status = AppointmentStatus.Completed;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.Appointment).ThenInclude(a => a!.Patient)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (prescription == null) return NotFound();
            return View(prescription);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription != null)
            {
                _context.Prescriptions.Remove(prescription);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}