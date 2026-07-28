using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MedCore.Data;
using MedCore.Models;
using MedCore.Services;
using MedCore.ViewModels;

namespace MedCore.Controllers
{
    [Authorize]
    public class LabOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public LabOrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task LoadDropdowns()
        {
            var patientsQuery = _context.Patients.AsQueryable();

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
                    patientsQuery = patientsQuery.Where(p => myPatientIds.Contains(p.Id));
                }
            }

            ViewBag.Patients = new SelectList(await patientsQuery.ToListAsync(), "Id", "FullName");
            ViewBag.Doctors = new SelectList(await _context.Doctors.ToListAsync(), "Id", "FullName");
        }

        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.LabOrders
                .Include(o => o.Patient).Include(o => o.Doctor)
                .AsQueryable();

            if (User.IsInRole("Doctor"))
            {
                var userId = _userManager.GetUserId(User);
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (doctor != null)
                    query = query.Where(o => o.DoctorId == doctor.Id);
            }

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(o => o.Patient!.FullName.Contains(search) || o.Doctor!.FullName.Contains(search));

            var orders = await query.OrderByDescending(o => o.OrderedOn).ToListAsync();
            ViewData["Search"] = search;
            return View(orders);
        }

        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();

            var model = new LabOrderViewModel();

            if (User.IsInRole("Doctor"))
            {
                var userId = _userManager.GetUserId(User);
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (doctor != null)
                    model.DoctorId = doctor.Id;
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Create(LabOrderViewModel model)
        {
            if (User.IsInRole("Doctor"))
            {
                var userId = _userManager.GetUserId(User);
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (doctor != null)
                    model.DoctorId = doctor.Id;
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdowns();
                return View(model);
            }

            var patient = await _context.Patients.FindAsync(model.PatientId);
            var doctorEntity = await _context.Doctors.FindAsync(model.DoctorId);
            if (patient == null || doctorEntity == null) return NotFound();

            var order = new LabOrder
            {
                PatientId = model.PatientId,
                DoctorId = model.DoctorId,
                TestCode = model.TestCode,
                TestName = model.TestName
            };

            _context.LabOrders.Add(order);
            await _context.SaveChangesAsync();

            order.Hl7OrderMessage = Hl7Helper.BuildOrderMessage(order, patient, doctorEntity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.LabOrders
                .Include(o => o.Patient).Include(o => o.Doctor)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();
            return View(order);
        }

        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> ReceiveResult(int id)
        {
            var order = await _context.LabOrders.FindAsync(id);
            if (order == null) return NotFound();
            return View(new ReceiveResultViewModel { LabOrderId = id });
        }

        [HttpPost]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> ReceiveResult(ReceiveResultViewModel model)
        {
            var order = await _context.LabOrders.FindAsync(model.LabOrderId);
            if (order == null) return NotFound();

            if (!ModelState.IsValid) return View(model);

            var resultValue = Hl7Helper.ParseResultValue(model.Hl7ResultMessage);
            if (resultValue == null)
            {
                ModelState.AddModelError(string.Empty, "Could not find an OBX segment with a result value in the message.");
                return View(model);
            }

            order.Hl7ResultMessage = model.Hl7ResultMessage;
            order.ResultValue = resultValue;
            order.ResultReceivedOn = DateTime.UtcNow;
            order.Status = LabOrderStatus.ResultReceived;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = order.Id });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.LabOrders
                .Include(o => o.Patient).Include(o => o.Doctor)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.LabOrders.FindAsync(id);
            if (order == null) return NotFound();
            _context.LabOrders.Remove(order);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}