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
        public int MyPatientsCount { get; set; }       
        public int MyPendingPrescriptions { get; set; }  
        public int NewPatientsThisWeek { get; set; }    
        public int PendingLabOrders { get; set; }    
        public int UnpaidBillsCount { get; set; }       
    }
}