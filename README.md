# Appointra — Appointment Management System

A full-stack appointment scheduling platform built with ASP.NET Core MVC. Designed for salons, clinics, fitness studios, spas, and any service-based business that needs to manage staff, clients, and bookings in one place.

This project was built as a university coursework submission for learning ASP.NET Core, Entity Framework Core, and the MVC pattern with authentication and role-based access control.

---

## Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Screenshots](#screenshots)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [Database](#database)
- [Default Accounts](#default-accounts)
- [Authentication & Authorization](#authentication--authorization)
- [License](#license)

---

## Features

**Public-facing**
- Marketing landing page with feature sections, industry cards, integrations showcase, and social proof
- Responsive design that works across desktop, tablet, and mobile
- Scroll-reveal animations and micro-interactions throughout

**Admin & Staff Dashboard**
- Personalized greeting with time-of-day awareness
- At-a-glance stat cards (total, scheduled, completed, cancelled, today's appointments, staff count)
- Recent appointments table with status badges
- Quick-action shortcuts for common tasks

**Appointment Management**
- Full CRUD — create, view, edit, and delete appointments
- Client details: name, email, phone
- Scheduling: start time, duration, assigned staff member
- Status tracking: Scheduled, Completed, Cancelled, No-Show
- Optional notes field

**Staff Management**
- Full CRUD for team members
- Fields: name, email, phone, specialty, active/inactive status
- Staff assignment to appointments via dropdown

**Authentication & Security**
- Cookie-based authentication with SHA-256 password hashing
- Role-based authorization (Admin, Staff)
- Dynamic sidebar navigation loaded from the database per user role
- Login, logout, access denied pages
- Session timeout (30 minutes)

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Core MVC (.NET 10) |
| ORM | Entity Framework Core 10 |
| Database | SQLite |
| Frontend | Vanilla CSS (custom design system), vanilla JS |
| Fonts | Fraunces (serif) + Inter (sans-serif) via Google Fonts |
| Icons | Inline SVGs (Lucide-style) |
| Images | Unsplash (landing page only) |

No Bootstrap, no jQuery UI, no Tailwind — the entire frontend is a hand-written design system using CSS custom properties.

---

## Screenshots

> Launch the app and visit `http://localhost:5090` to see the landing page, or log in to access the dashboard.

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (or later)
- A terminal (macOS Terminal, Windows Terminal, or VS Code integrated terminal)

### Run the project

```bash
# Clone the repository
git clone <your-repo-url>
cd AppointmentSystem.Web

# Restore dependencies and run
dotnet run
```

The app starts on **http://localhost:5090** by default.

On first launch, the database is created automatically and seeded with default roles, users, and menu items. No manual migration steps required.

### Optional: specify a custom port

```bash
dotnet run --urls http://localhost:3000
```

---

## Project Structure

```
AppointmentSystem.Web/
├── Controllers/
│   ├── AccountController.cs        # Login, logout, access denied
│   ├── AppointmentsController.cs   # Appointment CRUD
│   ├── DashboardController.cs      # Dashboard with stats
│   ├── HomeController.cs           # Landing page, privacy
│   └── StaffsController.cs         # Staff CRUD
├── Data/
│   ├── ApplicationDbContext.cs     # EF Core DbContext
│   ├── DbSeeder.cs                 # Seeds roles, users, menus on startup
│   ├── *Configuration.cs           # Entity-specific Fluent API configs
├── Filters/
│   └── MenuLoaderFilter.cs         # Loads user menus into ViewData per request
├── Migrations/                     # EF Core migration files
├── Models/
│   ├── Appointment.cs              # Appointment entity
│   ├── Staff.cs                    # Staff entity
│   ├── User.cs / Role.cs           # Auth entities
│   ├── UserRole.cs / UserMenu.cs   # Many-to-many join tables
│   ├── Menu.cs                     # Dynamic navigation items
│   ├── DashboardViewModel.cs       # Dashboard stats ViewModel
│   ├── LoginViewModel.cs           # Login form ViewModel
│   └── ErrorViewModel.cs           # Error page ViewModel
├── Views/
│   ├── Account/                    # Login, AccessDenied
│   ├── Appointments/               # Index, Create, Edit, Details, Delete
│   ├── Dashboard/                  # Dashboard Index
│   ├── Home/                       # Landing page, Privacy
│   ├── Shared/                     # Layout, error, validation partials
│   └── Staffs/                     # Index, Create, Edit, Details, Delete
├── wwwroot/
│   ├── css/site.css                # Complete design system (~2100 lines)
│   └── js/site.js                  # Mobile nav, scroll animations, table UX
├── Program.cs                      # App startup, DI, middleware pipeline
├── appsettings.json                # DB connection string
└── AppointmentSystem.Web.csproj    # Project file & NuGet references
```

---

## Database

The app uses **SQLite** — no database server installation needed. The database file is stored at:

```
../.db/appointment_system.db
```

(One directory above the project root, in a `.db/` folder)

Entity Framework Core handles schema creation via migrations. The `DbSeeder` class populates initial data on startup if the tables are empty.

### Entities

| Entity | Purpose |
|--------|---------|
| `Staff` | Team members who can be assigned appointments |
| `Appointment` | Client bookings with date, duration, status, and notes |
| `User` | Login credentials (username + hashed password) |
| `Role` | Authorization groups (Admin, Staff) |
| `UserRole` | Maps users to roles (many-to-many) |
| `Menu` | Sidebar navigation items |
| `UserMenu` | Maps menus to users (many-to-many) |

---

## Default Accounts

The seed data creates two accounts on first run:

| Username | Password | Role | Access |
|----------|----------|------|--------|
| `admin` | `admin123` | Admin | Full access: dashboard, appointments, staff |
| `staff` | `staff123` | Staff | Limited: dashboard, appointments only |

Passwords are stored as SHA-256 hashes. These are development-only defaults — change them for any real deployment.

---

## Authentication & Authorization

- Authentication uses ASP.NET Core **cookie authentication** (no external identity providers).
- Passwords are hashed with SHA-256 before storage (see `AccountController.HashPassword()`).
- The `[Authorize]` attribute protects all controllers except `Home` and `Account/Login`.
- The `[Authorize(Roles = "Admin")]` attribute restricts staff management to admins.
- `MenuLoaderFilter` is a global action filter that queries the user's assigned menu items from the database and injects them into `ViewData["UserMenus"]` on every request, so the sidebar renders dynamically based on role.

---

## Design System

The frontend uses a completely custom CSS design system (no framework dependencies):

- **Color palette**: Deep forest green (`#1a3a34`) primary, warm cream (`#f7f4ee`) background, accent greens
- **Typography**: Fraunces for headings and brand, Inter for body text
- **Components**: Cards, stat cards, tables, forms, badges, buttons, accordions, detail grids, empty states
- **Landing page**: Hero section, social proof, feature cards, industry showcase, detail rows with accordions, integrations bar, CTA, footer
- **Animations**: Scroll-reveal via IntersectionObserver, page entrance transitions, hover micro-interactions
- **Accessibility**: `focus-visible` outlines, `prefers-reduced-motion` support, keyboard-navigable sidebar
- **Responsive**: Breakpoints at 992px (tablet) and 768px (mobile), collapsible sidebar with overlay

---

## License

This project was created for educational purposes as part of a university assignment. Feel free to use it as a reference or starting point for your own projects.
