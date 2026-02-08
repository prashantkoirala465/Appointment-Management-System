# 📅 Appointment Management System

A simple, clean appointment booking system built with ASP.NET Core MVC. Perfect for small clinics, consultancies, or any business that needs to manage appointments with clients.

## ✨ What It Does

- **Manage Staff** - Add and manage your team members who take appointments
- **Book Appointments** - Schedule appointments with client details, dates, and durations
- **Track Status** - Keep tabs on whether appointments are scheduled, completed, or cancelled
- **View Everything** - Clean, easy-to-read lists of all your staff and appointments

## 🛠️ Built With

- **ASP.NET Core 8.0** - The web framework
- **Entity Framework Core** - For talking to the database
- **SQLite** - Lightweight database (no setup needed!)
- **Bootstrap 5** - Makes everything look nice and responsive

## 🚀 Getting Started

1. **Clone the repo**
   ```bash
   git clone <your-repo-url>
   cd AppointmentSystem.Web
   ```

2. **Run the app**
   ```bash
   dotnet run
   ```

3. **Open your browser**
   - Navigate to `https://localhost:5090` (or whatever port it shows)
   - Start adding staff and booking appointments!

## 📦 Database

The app uses SQLite, so no database server installation needed. The database file (`appointment_system.db`) is created automatically when you first run the app.

## 🎯 Features

- ✅ Create, view, edit, and delete staff members
- ✅ Book appointments with client contact information
- ✅ Set appointment duration and status
- ✅ Soft delete for staff with existing appointments
- ✅ Clean, responsive UI that works on mobile
- ✅ Form validation to keep data clean

## 📝 Notes

- Staff members with appointments can't be hard-deleted (they're marked as inactive instead)
- All timestamps are stored in UTC to avoid timezone headaches
- The system uses GUIDs for IDs (more secure than sequential numbers)

---

Made with ☕ and ASP.NET Core
