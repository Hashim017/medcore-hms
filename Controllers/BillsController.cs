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
    public class BillsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager; // ADDED

        public BillsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager) // ADDED param
        {
            _context = context;
            _userManager = userManager; // ADDED
        }

        private async Task LoadAppointments(int? selectedId = null)
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient).Include(a => a.Doctor)
                .Where(a => a.Bill == null)
                .ToListAsync();
            ViewBag.Appointments = new SelectList(
                appointments.Select(a => new { a.Id, Label = $"{a.Patient!.FullName} - {a.Doctor!.FullName} ({a.AppointmentDate:g})" }),
                "Id", "Label", selectedId);
        }

        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            int pageSize = 10;
            var query = _context.Bills
                .Include(b => b.Appointment).ThenInclude(a => a!.Patient)
                .Include(b => b.Appointment).ThenInclude(a => a!.Doctor) // ADDED so DoctorId filter works
                .OrderByDescending(b => b.GeneratedOn)
                .AsQueryable();

            // ADDED: Doctor sees only their own patients' unpaid bills
            if (User.IsInRole("Doctor"))
            {
                var userId = _userManager.GetUserId(User);
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (doctor != null)
                    query = query.Where(b => b.Appointment!.DoctorId == doctor.Id
                        && b.Status == PaymentStatus.Unpaid);
            }

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(b => b.Appointment!.Patient!.FullName.Contains(search));

            int totalCount = await query.CountAsync();
            var bills = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            ViewData["Search"] = search;
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalCount / (double)pageSize);
            return View(bills);
        }

        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Create()
        {
            await LoadAppointments();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Create(BillViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadAppointments(model.AppointmentId);
                return View(model);
            }

            _context.Bills.Add(new Bill
            {
                AppointmentId = model.AppointmentId,
                Amount = model.Amount,
                Status = model.Status
            });
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> MarkPaid(int id)
        {
            var bill = await _context.Bills.FindAsync(id);
            if (bill != null)
            {
                bill.Status = PaymentStatus.Paid;
                bill.PaidOn = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var bill = await _context.Bills
                .Include(b => b.Appointment).ThenInclude(a => a!.Patient)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (bill == null) return NotFound();
            return View(bill);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bill = await _context.Bills.FindAsync(id);
            if (bill != null)
            {
                _context.Bills.Remove(bill);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}