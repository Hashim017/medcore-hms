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
    public class DoctorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public DoctorsController(ApplicationDbContext context) => _context = context;

        private async Task LoadDepartments(int? selectedId = null)
        {
            ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name", selectedId);
        }

        public async Task<IActionResult> Index()
        {
            var doctors = await _context.Doctors.Include(d => d.Department).ToListAsync();
            return View(doctors);
        }

        public async Task<IActionResult> Details(int id)
        {
            var doctor = await _context.Doctors.Include(d => d.Department).FirstOrDefaultAsync(d => d.Id == id);
            if (doctor == null) return NotFound();
            return View(doctor);
        }

        public async Task<IActionResult> Create()
        {
            await LoadDepartments();
            return View();
        }

        [HttpPost]
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

        public async Task<IActionResult> Delete(int id)
        {
            var doctor = await _context.Doctors.Include(d => d.Department).FirstOrDefaultAsync(d => d.Id == id);
            if (doctor == null) return NotFound();
            return View(doctor);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor != null)
            {
                _context.Doctors.Remove(doctor);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}