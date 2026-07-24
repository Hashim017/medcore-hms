# MedCore — Clinic Management System

A multi-role Clinic Management System built with ASP.NET Core MVC, demonstrating enterprise software patterns relevant to healthcare domain applications.

## Tech Stack
- ASP.NET Core MVC (.NET 8)
- Entity Framework Core + SQL Server
- ASP.NET Core Identity (role-based authentication: Admin, Doctor, Receptionist)
- Bootstrap 5

## Features
- **Authentication & Authorization** — ASP.NET Identity with role-based access control enforced at the controller level (not just hidden UI)
- **Patient Management** — registration, search, pagination
- **Doctor Management** — department assignment, admin-controlled account linking
- **Appointment Scheduling** — conflict detection prevents double-booking a doctor at the same time slot
- **Prescriptions & Billing** — tied to completed appointments via one-to-one relationships
- **Role-restricted workflows**:
  - Admin — full access across all modules
  - Receptionist — manages patients, books appointments, generates/collects bills
  - Doctor — views only their own linked appointments, writes prescriptions, cannot manage patients/doctors
- **Admin-controlled identity linking** — Doctor login accounts are linked to Doctor profiles by an Admin (not self-selected at registration), preventing identity spoofing
- Search and pagination across all major data tables
- Referential integrity enforced at the database level (restricted deletes on records with dependent data), with friendly error handling instead of raw exceptions

## Architecture
- **ViewModels** separate form input from EF entities (validation lives here, not on domain models)
- **Explicit relationship configuration** in `ApplicationDbContext` for ambiguous one-to-one relationships (Appointment?Prescription, Appointment?Bill)
- **DeleteBehavior.Restrict** on Doctor/Patient foreign keys — protects appointment history from silent data loss
- **Role seeding + demo data seeding** on startup for consistent environment setup

## Setup
1. Clone the repo
2. Update the connection string in `appsettings.json` if your SQL Server instance differs
3. Run migrations:

dotnet ef database update

4. Run the app:

dotnet run

5. Register an account, or use the seeded demo data (25 patients, 12 doctors, 40 appointments) to explore immediately

## Why these design choices
This project was built to demonstrate transferable software engineering fundamentals — clean architecture, proper authorization boundaries, and real business logic (conflict detection, referential integrity) — using an enterprise stack (C#, ASP.NET Core, SQL Server) relevant to healthcare software teams.