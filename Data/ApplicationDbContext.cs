using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MedCore.Models;

namespace MedCore.Data
{
    // Inheriting IdentityDbContext gives us Users/Roles tables for free,
    // alongside our own tables below
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<Doctor> Doctors => Set<Doctor>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<Prescription> Prescriptions => Set<Prescription>();
        public DbSet<Bill> Bills => Set<Bill>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Doctor/Patient deletion is blocked if they have appointments —
            // protects appointment history from disappearing by accident
            builder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // One appointment -> one prescription
            builder.Entity<Appointment>()
                .HasOne(a => a.Prescription)
                .WithOne(p => p.Appointment)
                .HasForeignKey<Prescription>(p => p.AppointmentId);

            // One appointment -> one bill
            builder.Entity<Appointment>()
                .HasOne(a => a.Bill)
                .WithOne(b => b.Appointment)
                .HasForeignKey<Bill>(b => b.AppointmentId);

            builder.Entity<Doctor>()
                .HasOne(d => d.Department)
                .WithMany(dept => dept.Doctors)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Doctor>()
                .HasOne(d => d.ApplicationUser)
                .WithOne(u => u.Doctor)
                .HasForeignKey<ApplicationUser>(u => u.DoctorId);
        }
    }
}