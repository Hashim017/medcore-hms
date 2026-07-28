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
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.UserName = User.Identity?.Name;
            ViewBag.UserRole = User.IsInRole("Doctor") ? "Doctor"
                : User.IsInRole("Receptionist") ? "Receptionist"
                : "Admin";

            var userId = _userManager.GetUserId(User);
            Doctor? doctor = null;

            if (User.IsInRole("Doctor"))
            {
                doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                var appUser = await _userManager.GetUserAsync(User);
                ViewBag.DisplayName = !string.IsNullOrWhiteSpace(doctor?.FullName)
                    ? doctor.FullName
                    : (appUser?.FullName ?? User.Identity?.Name);
            }
            else
            {
                var appUser = await _userManager.GetUserAsync(User);
                ViewBag.DisplayName = appUser?.FullName ?? User.Identity?.Name;
            }

            var today = DateTime.Today;
            var apptQuery = _context.Appointments.Include(a => a.Patient).Include(a => a.Doctor).AsQueryable();
            var billQuery = _context.Bills.Include(b => b.Appointment).AsQueryable();

            if (User.IsInRole("Doctor") && doctor != null)
            {
                apptQuery = apptQuery.Where(a => a.DoctorId == doctor.Id);
                billQuery = billQuery.Where(b => b.Appointment!.DoctorId == doctor.Id);
            }

            var vm = new DashboardViewModel
            {
                TotalPatients = await _context.Patients.CountAsync(),
                TotalDoctors = await _context.Doctors.CountAsync(),
                AppointmentsToday = await apptQuery.CountAsync(a => a.AppointmentDate.Date == today),
                PendingAppointments = await apptQuery.CountAsync(a => a.Status == AppointmentStatus.Pending),
                UnpaidBillsTotal = await billQuery
                    .Where(b => b.Status == PaymentStatus.Unpaid)
                    .SumAsync(b => (decimal?)b.Amount) ?? 0,
                RecentAppointments = await apptQuery
                    .OrderByDescending(a => a.AppointmentDate)
                    .Take(5)
                    .ToListAsync()
            };

            // Role-specific extras
            if (User.IsInRole("Doctor") && doctor != null)
            {
                vm.MyPatientsCount = await _context.Appointments
                    .Where(a => a.DoctorId == doctor.Id)
                    .Select(a => a.PatientId)
                    .Distinct()
                    .CountAsync();

                vm.MyPendingPrescriptions = await _context.Prescriptions
                    .CountAsync(p => p.Appointment!.DoctorId == doctor.Id);
            }

            if (User.IsInRole("Receptionist"))
            {
                vm.NewPatientsThisWeek = await _context.Patients
                    .CountAsync(p => p.RegisteredOn >= today.AddDays(-7));

                vm.PendingLabOrders = await _context.LabOrders
                    .CountAsync(l => l.Status == LabOrderStatus.Ordered);

                vm.UnpaidBillsCount = await _context.Bills
                    .CountAsync(b => b.Status == PaymentStatus.Unpaid);
            }

            return View(vm);
        }
    }
}