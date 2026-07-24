using MedCore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

        public static async Task SeedDemoDataAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var rnd = new Random();

            string[] firstNames = { "Ahmed", "Sara", "Bilal", "Ayesha", "Usman", "Hina", "Zain", "Mahnoor", "Faisal", "Rabia", "Adeel", "Sana", "Kashif", "Nida", "Waqas", "Iqra", "Hamza", "Sadia", "Tariq", "Mehak" };
            string[] lastNames = { "Khan", "Raza", "Ahmed", "Malik", "Iqbal", "Sheikh", "Butt", "Chaudhry", "Qureshi", "Farooq" };
            string[] cities = { "Lahore", "Karachi", "Islamabad", "Faisalabad", "Multan", "Rawalpindi" };

            if (!db.Patients.Any())
            {
                var patients = new List<Patient>();
                for (int i = 0; i < 25; i++)
                {
                    patients.Add(new Patient
                    {
                        FullName = $"{firstNames[rnd.Next(firstNames.Length)]} {lastNames[rnd.Next(lastNames.Length)]}",
                        DateOfBirth = new DateTime(rnd.Next(1960, 2015), rnd.Next(1, 13), rnd.Next(1, 28)),
                        Gender = rnd.Next(0, 2) == 0 ? "Male" : "Female",
                        PhoneNumber = $"03{rnd.Next(0, 9)}{rnd.Next(10000000, 99999999)}",
                        Email = $"patient{i}@example.com",
                        Address = cities[rnd.Next(cities.Length)]
                    });
                }
                db.Patients.AddRange(patients);
                await db.SaveChangesAsync();
            }

            if (!db.Doctors.Any())
            {
                var departments = await db.Departments.ToListAsync();
                string[] specializations = { "Cardiologist", "Pediatrician", "General Physician", "Dermatologist", "Orthopedic", "ENT Specialist" };

                var doctors = new List<Doctor>();
                for (int i = 0; i < 12; i++)
                {
                    doctors.Add(new Doctor
                    {
                        FullName = $"Dr. {firstNames[rnd.Next(firstNames.Length)]} {lastNames[rnd.Next(lastNames.Length)]}",
                        Specialization = specializations[rnd.Next(specializations.Length)],
                        PhoneNumber = $"03{rnd.Next(0, 9)}{rnd.Next(10000000, 99999999)}",
                        DepartmentId = departments[rnd.Next(departments.Count)].Id
                    });
                }
                db.Doctors.AddRange(doctors);
                await db.SaveChangesAsync();
            }

            if (!db.Appointments.Any())
            {
                var patients = await db.Patients.ToListAsync();
                var doctors = await db.Doctors.ToListAsync();

                var appointments = new List<Appointment>();
                for (int i = 0; i < 40; i++)
                {
                    appointments.Add(new Appointment
                    {
                        PatientId = patients[rnd.Next(patients.Count)].Id,
                        DoctorId = doctors[rnd.Next(doctors.Count)].Id,
                        AppointmentDate = DateTime.Today.AddDays(rnd.Next(-20, 20)).AddHours(rnd.Next(9, 17)),
                        Status = (AppointmentStatus)rnd.Next(0, 4),
                        Notes = "Routine checkup"
                    });
                }
                db.Appointments.AddRange(appointments);
                await db.SaveChangesAsync();
            }

            if (!db.Bills.Any())
            {
                var completedAppointments = await db.Appointments
                    .Where(a => a.Status == AppointmentStatus.Completed)
                    .ToListAsync();

                foreach (var appt in completedAppointments)
                {
                    db.Bills.Add(new Bill
                    {
                        AppointmentId = appt.Id,
                        Amount = rnd.Next(1500, 8000),
                        Status = rnd.Next(0, 2) == 0 ? PaymentStatus.Unpaid : PaymentStatus.Paid
                    });
                }
                await db.SaveChangesAsync();
            }

            if (!db.Prescriptions.Any())
            {
                string[] medicines = { "Paracetamol 500mg", "Amoxicillin 250mg", "Ibuprofen 400mg", "Omeprazole 20mg", "Cetirizine 10mg" };
                string[] instructions = { "Take twice daily after meals", "Take once daily before bed", "Take three times a day", "Take as needed for pain" };

                var completedAppointments = await db.Appointments
                    .Where(a => a.Status == AppointmentStatus.Completed)
                    .ToListAsync();

                foreach (var appt in completedAppointments)
                {
                    db.Prescriptions.Add(new Prescription
                    {
                        AppointmentId = appt.Id,
                        Medicines = medicines[rnd.Next(medicines.Length)],
                        Instructions = instructions[rnd.Next(instructions.Length)]
                    });
                }
                await db.SaveChangesAsync();
            }
        }
    }
}