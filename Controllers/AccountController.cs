using MedCore.Data;
using MedCore.Helpers;
using MedCore.Models;
using MedCore.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MedCore.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (model.Role == "Doctor" && (string.IsNullOrWhiteSpace(model.Specialization) || model.DepartmentId == null))
                ModelState.AddModelError(string.Empty, "Specialization and Department are required for Doctor accounts.");

            if (!ModelState.IsValid)
            {
                ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = StringHelpers.ToTitleCase(model.FullName)
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)   
                    ModelState.AddModelError(string.Empty, error.Description);
                ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name");
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, model.Role);

            // Auto-create the bookable Doctor profile, linked to this account
            if (model.Role == "Doctor")
            {
                var doctor = new Doctor
                {
                    FullName = StringHelpers.ToTitleCase(model.FullName),
                    Specialization = model.Specialization!,
                    PhoneNumber = model.PhoneNumber,
                    DepartmentId = model.DepartmentId!.Value,
                    UserId = user.Id
                };
                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Dashboard");
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            var roles = await _userManager.GetRolesAsync(user!);

            if (roles.Contains("Admin"))
                return RedirectToAction("Index", "Dashboard");
            if (roles.Contains("Doctor"))
                return RedirectToAction("Index", "Dashboard");

            return RedirectToAction("Index", "Dashboard"); // we'll change these once other controllers exist
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}