using Microsoft.AspNetCore.Identity;
using MedCore.Models;

namespace MedCore.Data
{
    public static class RoleSeeder
    {
        public static readonly string[] Roles = { "Admin", "Doctor", "Receptionist" };
        public static readonly string[] Departments = { "Cardiology", "Pediatrics", "General Medicine", "Dermatology" };

        public static async Task SeedRolesAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var role in Roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            if (!db.Departments.Any())
            {
                foreach (var name in Departments)
                    db.Departments.Add(new Department { Name = name });
                await db.SaveChangesAsync();
            }
        }
    }
}