using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedCore.Data;
using MedCore.Models;
using MedCore.ViewModels;

namespace MedCore.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        public DashboardController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;

            var vm = new DashboardViewModel
            {
                TotalPatients = await _context.Patients.CountAsync(),
                TotalDoctors = await _context.Doctors.CountAsync(),
                AppointmentsToday = await _context.Appointments
                    .CountAsync(a => a.AppointmentDate.Date == today),
                PendingAppointments = await _context.Appointments
                    .CountAsync(a => a.Status == AppointmentStatus.Pending),
                UnpaidBillsTotal = await _context.Bills
                    .Where(b => b.Status == PaymentStatus.Unpaid)
                    .SumAsync(b => (decimal?)b.Amount) ?? 0,
                RecentAppointments = await _context.Appointments
                    .Include(a => a.Patient).Include(a => a.Doctor)
                    .OrderByDescending(a => a.AppointmentDate)
                    .Take(5)
                    .ToListAsync()
            };

            return View(vm);
        }
    }
}