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
    public class BillsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public BillsController(ApplicationDbContext context) => _context = context;

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

        public async Task<IActionResult> Index()
        {
            var bills = await _context.Bills
                .Include(b => b.Appointment).ThenInclude(a => a!.Patient)
                .ToListAsync();
            return View(bills);
        }

        public async Task<IActionResult> Create()
        {
            await LoadAppointments();
            return View();
        }

        [HttpPost]
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

        public async Task<IActionResult> Delete(int id)
        {
            var bill = await _context.Bills
                .Include(b => b.Appointment).ThenInclude(a => a!.Patient)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (bill == null) return NotFound();
            return View(bill);
        }

        [HttpPost, ActionName("Delete")]
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