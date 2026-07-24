using MedCore.Data;
using MedCore.Models;
using MedCore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MedCore.Controllers
{
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task LoadDropdowns(int? patientId = null, int? doctorId = null)
        {
            ViewBag.Patients = new SelectList(await _context.Patients.ToListAsync(), "Id", "FullName", patientId);
            ViewBag.Doctors = new SelectList(await _context.Doctors.ToListAsync(), "Id", "FullName", doctorId);
        }

        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            int pageSize = 10;

            var query = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .OrderByDescending(a => a.AppointmentDate)
                .AsQueryable();

            if (User.IsInRole("Doctor"))
            {
                var userId = _userManager.GetUserId(User);
                query = query.Where(a => a.Doctor!.ApplicationUserId == userId);
            }

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(a => a.Patient!.FullName.Contains(search) || a.Doctor!.FullName.Contains(search));

            int totalCount = await query.CountAsync();
            var appointments = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewData["Search"] = search;
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalCount / (double)pageSize);

            return View(appointments);
        }

        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Receptionist")]
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
        [Authorize(Roles = "Admin,Receptionist")]
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
        [Authorize(Roles = "Admin,Receptionist")]
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

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var appt = await _context.Appointments
                .Include(a => a.Patient).Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (appt == null) return NotFound();
            return View(appt);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
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

        public async Task<IActionResult> Details(int id)
        {
            var appt = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Prescription)
                .Include(a => a.Bill)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appt == null) return NotFound();

            // Doctor can only view their own appointment details
            if (User.IsInRole("Doctor"))
            {
                var userId = _userManager.GetUserId(User);
                if (appt.Doctor?.ApplicationUserId != userId) return Forbid();
            }

            return View(appt);
        }
    }
}