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
    public class DoctorsController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public DoctorsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task LoadDepartments(int? selectedId = null)
        {
            ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name", selectedId);
        }

        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            int pageSize = 10;
            var query = _context.Doctors.Include(d => d.Department).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(d => d.FullName.Contains(search) || d.Specialization.Contains(search));

            int totalCount = await query.CountAsync();
            var doctors = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewData["Search"] = search;
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalCount / (double)pageSize);

            return View(doctors);
        }

        public async Task<IActionResult> Details(int id)
        {
            var doctor = await _context.Doctors.Include(d => d.Department).FirstOrDefaultAsync(d => d.Id == id);
            if (doctor == null) return NotFound();
            return View(doctor);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            await LoadDepartments();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(DoctorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadDepartments(model.DepartmentId);
                return View(model);
            }

            var doctor = new Doctor
            {
                FullName = model.FullName,
                Specialization = model.Specialization,
                PhoneNumber = model.PhoneNumber,
                DepartmentId = model.DepartmentId
            };
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return NotFound();

            await LoadDepartments(doctor.DepartmentId);
            return View(new DoctorViewModel
            {
                Id = doctor.Id,
                FullName = doctor.FullName,
                Specialization = doctor.Specialization,
                PhoneNumber = doctor.PhoneNumber,
                DepartmentId = doctor.DepartmentId
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, DoctorViewModel model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid)
            {
                await LoadDepartments(model.DepartmentId);
                return View(model);
            }

            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return NotFound();

            doctor.FullName = model.FullName;
            doctor.Specialization = model.Specialization;
            doctor.PhoneNumber = model.PhoneNumber;
            doctor.DepartmentId = model.DepartmentId;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Delete(int id)
        {
            var doctor = await _context.Doctors.Include(d => d.Department).FirstOrDefaultAsync(d => d.Id == id);
            if (doctor == null) return NotFound();
            return View(doctor);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor != null)
            {
                try
                {
                    _context.Doctors.Remove(doctor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    TempData["Error"] = "Cannot delete this doctor — they have existing appointments linked to their record.";
                    return RedirectToAction(nameof(Index));
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}