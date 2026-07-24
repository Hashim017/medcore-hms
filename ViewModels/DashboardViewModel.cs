using MedCore.Models;

namespace MedCore.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalPatients { get; set; }
        public int TotalDoctors { get; set; }
        public int AppointmentsToday { get; set; }
        public int PendingAppointments { get; set; }
        public decimal UnpaidBillsTotal { get; set; }
        public List<Appointment> RecentAppointments { get; set; } = new();
    }
}