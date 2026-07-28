# MedCore: Clinic Management System

A multi-role Clinic Management System built with ASP.NET Core MVC, demonstrating enterprise software patterns relevant to healthcare domain applications. The project includes HL7 messaging, role-scoped data access, and clinical workflow logic.

## Tech Stack

- ASP.NET Core MVC, .NET 8
- Entity Framework Core with SQL Server
- ASP.NET Core Identity for role-based authentication across Admin, Doctor, and Receptionist roles
- Bootstrap 5 with a custom clinical design system using Space Grotesk, Inter, and JetBrains Mono

## Features

### Authentication and Authorization
- ASP.NET Identity with role-based access control enforced at the controller level, not just hidden in the UI
- Self-service doctor registration with auto-linked profiles. A new doctor account is automatically tied to its Doctor record on signup, so no manual admin linking step is required

### Core Modules
- **Patient Management**: registration, search, and pagination
- **Doctor Management**: department assignment, specialization, and linked login account
- **Appointment Scheduling**: conflict detection prevents double-booking a doctor at the same time slot
- **Prescriptions**: tied to completed appointments through a one-to-one relationship
- **Billing**: tied to completed appointments, tracks unpaid and paid status, and displays consistently in USD regardless of server locale
- **Lab Orders**: order tests, generate and parse simulated HL7 order and result messages, and track order status as Ordered or Result Received
- **FHIR-Style API**: read-only REST endpoints at /fhir/Patient/id and /fhir/Appointment/id that expose internal data shaped as FHIR Patient and Appointment resources, supporting interoperability with external healthcare systems

### Role-Based Dashboards
Each role sees stats relevant to their day-to-day work instead of one generic view.
- **Admin**: total patients, total doctors, pending appointments across all doctors, and unpaid bills total
- **Doctor**: own patient count, own prescriptions written, own pending appointments, and own unpaid bills
- **Receptionist**: pending appointments, new patients this week, pending lab orders, and unpaid bills count

### Role-Scoped Data Access
Beyond page-level authorization, data itself is scoped per doctor across the app.
- Doctors only see their own patients, appointments, prescriptions, bills, and lab orders. They never see another doctor's data
- Dropdowns such as the one used for ordering a lab test only list a doctor's own patients, and the ordering doctor is auto-selected and locked so orders cannot be submitted under another doctor's name

### Data Integrity and UX
- Referential integrity enforced at the database level restricts deletes on records with dependent data, and shows friendly error handling instead of raw exceptions
- Search and pagination available across all major data tables
- Names are auto-formatted on save so capitalization stays consistent regardless of input casing

## Architecture

- ViewModels separate form input from EF entities, so validation lives here rather than on domain models
- Explicit relationship configuration in ApplicationDbContext handles ambiguous one-to-one relationships between Appointment and Prescription, Appointment and Bill, and Doctor and ApplicationUser
- DeleteBehavior.Restrict is set on Doctor and Patient foreign keys to protect appointment history from silent data loss
- An HL7 message helper generates and parses simplified HL7-formatted order and result messages for the Lab Orders module
- Role seeding and demo data seeding run on startup, creating all three roles, sample departments, patients, and working login accounts for admin, doctors, and receptionists with fully linked, realistic relational data

## Setup

1. Clone the repo
2. Update the connection string in appsettings.json if your SQL Server instance differs
3. Run migrations:
   ```
   dotnet ef database update
   ```
4. Run the app:
   ```
   dotnet run
   ```
5. Log in with seeded demo accounts, or register your own:
   - Admin: admin@medcore.com, password Staff@123
   - Doctor: dr.reed@medcore.com, password Doctor@123
   - Receptionist: reception1@medcore.com, password Staff@123

## Screenshots

### Admin Dashboard
![Admin Dashboard](Screenshots/admin-dashboard.png)

### Doctor Dashboard
![Doctor Dashboard](Screenshots/doctor-dashboard.png)

### Receptionist Dashboard
![Receptionist Dashboard](Screenshots/receptionist-dashboard.png)

### Appointments List
![Appointments List](Screenshots/appointments-page.png)

### Bills List
![Bills List](Screenshots/bills-page.png)

### Lab Orders List
![Lab Orders List](Screenshots/lab-orders-page.png)

### Lab Order with HL7 Message
![Lab Order with HL7 Message](Screenshots/lab-order-with-received-obx-result.png)

### Prescriptions List
![Prescriptions List](Screenshots/prescriptions-page.png)

## Why these design choices

This project was built to demonstrate transferable software engineering fundamentals. That includes clean architecture, proper authorization boundaries, and real business logic such as conflict detection, referential integrity, and role-scoped data access, all using an enterprise stack of C#, ASP.NET Core, and SQL Server relevant to healthcare software teams. The Lab Orders module and its HL7 messaging, along with the FHIR-shaped API layer, demonstrate familiarity with healthcare interoperability standards used in real EHR and clinic systems.
