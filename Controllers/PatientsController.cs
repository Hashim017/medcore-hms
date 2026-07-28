using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedCore.Data;
using MedCore.Models;
using MedCore.ViewModels;

namespace MedCore.Controllers
{
    [Authorize]
    public class PatientsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager; // ADDED

        public PatientsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager) // ADDED param
        {
            _context = context;
            _userManager = userManager; // ADDED
        }

        // GET: /Patients
        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            int pageSize = 10;
            var query = _context.Patients.AsQueryable();

            // ADDED: Doctor sees only patients they've had appointments with
            if (User.IsInRole("Doctor"))
            {
                var userId = _userManager.GetUserId(User);
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (doctor != null)
                {
                    var myPatientIds = _context.Appointments
                        .Where(a => a.DoctorId == doctor.Id)
                        .Select(a => a.PatientId)
                        .Distinct();
                    query = query.Where(p => myPatientIds.Contains(p.Id));
                }
            }

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.FullName.Contains(search));

            int totalCount = await query.CountAsync();
            var patients = await query
                .OrderByDescending(p => p.RegisteredOn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["Search"] = search;
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalCount / (double)pageSize);

            return View(patients);
        }

        // GET: /Patients/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return NotFound();
            return View(patient);
        }

        // GET: /Patients/Create
        [Authorize(Roles = "Admin,Receptionist")]
        public IActionResult Create() => View();

        // POST: /Patients/Create
        [HttpPost]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Create(PatientViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var patient = new Patient
            {
                FullName = model.FullName,
                DateOfBirth = model.DateOfBirth,
                Gender = model.Gender,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
                Address = model.Address
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Patients/Edit/5
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Edit(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return NotFound();

            var model = new PatientViewModel
            {
                Id = patient.Id,
                FullName = patient.FullName,
                DateOfBirth = patient.DateOfBirth,
                Gender = patient.Gender,
                PhoneNumber = patient.PhoneNumber,
                Email = patient.Email,
                Address = patient.Address
            };
            return View(model);
        }

        // POST: /Patients/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Edit(int id, PatientViewModel model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return NotFound();

            patient.FullName = model.FullName;
            patient.DateOfBirth = model.DateOfBirth;
            patient.Gender = model.Gender;
            patient.PhoneNumber = model.PhoneNumber;
            patient.Email = model.Email;
            patient.Address = model.Address;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Patients/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return NotFound();
            return View(patient);
        }

        // POST: /Patients/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient != null)
            {
                try
                {
                    _context.Patients.Remove(patient);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    TempData["Error"] = "Cannot delete this patient — they have existing appointments linked to their record.";
                    return RedirectToAction(nameof(Index));
                }
            }
            return RedirectToAction(nameof(Index));
        }

    }
}