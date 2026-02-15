using System;
using System.ComponentModel;

namespace AppointmentSystem.Web.Models
{
    /// This view model carries all the data needed to display the dashboard
    /// It aggregates statistics from appointments, staff, and users
    /// Think of it as a snapshot of the system's current state
    public class DashboardViewModel
    {
        // --- Appointment Statistics ---

        // Total number of appointments in the system
        [DisplayName("Total Appointments")]
        public int TotalAppointments { get; set; }

        // Appointments that are currently scheduled (upcoming)
        [DisplayName("Scheduled")]
        public int ScheduledAppointments { get; set; }

        // Appointments that have been completed
        [DisplayName("Completed")]
        public int CompletedAppointments { get; set; }

        // Appointments that were cancelled
        [DisplayName("Cancelled")]
        public int CancelledAppointments { get; set; }

        // Appointments scheduled for today specifically
        [DisplayName("Today's Appointments")]
        public int TodayAppointments { get; set; }

        // --- Staff Statistics ---

        // Total number of staff members in the system
        [DisplayName("Total Staff")]
        public int TotalStaff { get; set; }

        // Staff members who are currently active
        [DisplayName("Active Staff")]
        public int ActiveStaff { get; set; }

        // --- User Statistics ---

        // Total number of user accounts in the system
        [DisplayName("Total Users")]
        public int TotalUsers { get; set; }

        // User accounts that are currently active
        [DisplayName("Active Users")]
        public int ActiveUsers { get; set; }

        // --- Recent Activity ---

        // The most recent appointments for quick reference
        // Shows a list of the latest bookings on the dashboard
        public List<Appointment> RecentAppointments { get; set; } = new();
    }
}
