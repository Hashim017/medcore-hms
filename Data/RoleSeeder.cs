using MedCore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MedCore.Services;

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
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var rnd = new Random();

            string[] firstNames = { "James", "Emily", "Michael", "Sophia", "William", "Olivia", "Daniel", "Emma", "David", "Grace", "Robert", "Charlotte", "Thomas", "Amelia", "Christopher", "Isabella", "Andrew", "Mia", "Joseph", "Ava" };
            string[] lastNames = { "Smith", "Johnson", "Williams", "Brown", "Taylor", "Anderson", "Wilson", "Clark", "Walker", "Hall" };
            string[] cities = { "New York", "London", "Toronto", "Sydney", "Manchester", "Chicago" };

            // 1. Seed Patients
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

            // 2. Seed 3 real Doctor logins, properly linked
            var doctorSeeds = new[]
            {
                new { First = "Jonathan", Last = "Reed", Email = "dr.reed@medcore.com", Spec = "Cardiologist" },
                new { First = "Sarah", Last = "Bennett", Email = "dr.bennett@medcore.com", Spec = "Pediatrician" },
                new { First = "Marcus", Last = "Coleman", Email = "dr.coleman@medcore.com", Spec = "General Physician" }
            };

            if (!db.Doctors.Any())
            {
                var departments = await db.Departments.ToListAsync();

                foreach (var d in doctorSeeds)
                {
                    // Create the login account first
                    var user = new ApplicationUser
                    {
                        UserName = d.Email,
                        Email = d.Email,
                        FullName = $"Dr. {d.First} {d.Last}",
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(user, "Doctor@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Doctor");

                        // Create the Doctor record and link it to the login
                        var doctor = new Doctor
                        {
                            FullName = $"Dr. {d.First} {d.Last}",
                            Specialization = d.Spec,
                            PhoneNumber = $"03{rnd.Next(0, 9)}{rnd.Next(10000000, 99999999)}",
                            DepartmentId = departments[rnd.Next(departments.Count)].Id,
                            UserId = user.Id // properly linked, no more NULL
                        };
                        db.Doctors.Add(doctor);
                    }
                }
                await db.SaveChangesAsync();
            }

            // 2b. Seed Admin and Receptionist logins
            var staffSeeds = new[]
            {
                new { First = "Michael", Last = "Turner", Email = "admin@medcore.com", Role = "Admin" },
                new { First = "Rachel", Last = "Adams", Email = "reception1@medcore.com", Role = "Receptionist" },
                new { First = "Laura", Last = "Mitchell", Email = "reception2@medcore.com", Role = "Receptionist" }
};

            foreach (var s in staffSeeds)
            {
                var existing = await userManager.FindByEmailAsync(s.Email);
                if (existing == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = s.Email,
                        Email = s.Email,
                        FullName = $"{s.First} {s.Last}",
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(user, "Staff@123");
                    if (result.Succeeded)
                        await userManager.AddToRoleAsync(user, s.Role);
                }
            }

            // 3. Seed Appointments (only across the 3 real doctors now)
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

            // 4. Bills
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
                        Amount = rnd.Next(50, 500),
                        Status = rnd.Next(0, 2) == 0 ? PaymentStatus.Unpaid : PaymentStatus.Paid
                    });
                }
                await db.SaveChangesAsync();
            }

            // 5. Prescriptions
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

            // 6. Lab Orders
            if (!db.LabOrders.Any())
            {
                string[] testCodes = { "CBC", "LFT", "RFT", "TSH", "HBA1C" };
                string[] testNames = { "Complete Blood Count", "Liver Function Test", "Renal Function Test", "Thyroid Stimulating Hormone", "Hemoglobin A1C" };

                var patients = await db.Patients.ToListAsync();
                var doctors = await db.Doctors.ToListAsync();

                var labOrders = new List<LabOrder>();
                for (int i = 0; i < 15; i++)
                {
                    var patient = patients[rnd.Next(patients.Count)];
                    var doctor = doctors[rnd.Next(doctors.Count)];
                    var testIndex = rnd.Next(testCodes.Length);
                    var isReceived = rnd.Next(0, 2) == 0;

                    var order = new LabOrder
                    {
                        PatientId = patient.Id,
                        DoctorId = doctor.Id,
                        TestCode = testCodes[testIndex],
                        TestName = testNames[testIndex],
                        Status = isReceived ? LabOrderStatus.ResultReceived : LabOrderStatus.Ordered,
                        OrderedOn = DateTime.UtcNow.AddDays(-rnd.Next(1, 15))
                    };
                    order.Hl7OrderMessage = Hl7Helper.BuildOrderMessage(order, patient, doctor);

                    if (isReceived)
                    {
                        order.ResultValue = $"{rnd.Next(4, 12)}.{rnd.Next(0, 9)}";
                        order.ResultReceivedOn = order.OrderedOn.AddDays(rnd.Next(1, 3));
                    }

                    labOrders.Add(order);
                }
                db.LabOrders.AddRange(labOrders);
                await db.SaveChangesAsync();
            }
        }
    }
}