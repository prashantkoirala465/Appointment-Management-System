# Appointra — Complete Implementation Guide

> **Project**: Appointment Management System (Appointra)
> **Course**: University Coursework — ASP.NET Core MVC
> **Team Members**: Prashant Koirala, Ananta, Anupam
> **Framework**: ASP.NET Core MVC (.NET 10) with Entity Framework Core
> **Date**: February 2026

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Team Responsibilities](#2-team-responsibilities)
3. [Tech Stack & Architecture](#3-tech-stack--architecture)
4. [Environment Setup & Commands](#4-environment-setup--commands)
5. [Project Structure Explained](#5-project-structure-explained)
6. [Database Design & Entity Relationships](#6-database-design--entity-relationships)
7. [Ananta's Module — Authentication & Security Layer](#7-anantas-module--authentication--security-layer)
8. [Prashant's Module — Appointment Management & REST API](#8-prashants-module--appointment-management--rest-api)
9. [Anupam's Module — Dashboard, Staff & UI Design System](#9-anupams-module--dashboard-staff--ui-design-system)
10. [How the Application Starts (Program.cs Walkthrough)](#10-how-the-application-starts-programcs-walkthrough)
11. [API Documentation & Testing](#11-api-documentation--testing)
12. [Common Cross-Questions & Answers](#12-common-cross-questions--answers)

---

## 1. Project Overview

**Appointra** is a full-stack appointment management system built for service-based businesses (salons, clinics, fitness studios, spas). It lets administrators manage staff, clients, and bookings from a single web application.

### What the App Does

- **Public Landing Page**: A marketing website introducing the product, visible to anyone
- **User Authentication**: Login with username/password OR Google OAuth
- **Role-Based Access**: Admin users get full control; Staff users get limited access
- **Dynamic Navigation**: Each user sees different sidebar menus based on what the admin has assigned to them
- **Appointment Management**: Create, view, edit, delete appointments — track client name, phone, email, assigned staff member, time, duration, status, and notes
- **Staff Management**: Admin can add, edit, deactivate/delete staff members
- **User Management**: Admin can create users, assign roles and menus, approve/reject registrations
- **Role & Menu Management**: Admin can create/edit roles and menu items
- **Dashboard**: A statistics overview showing appointment counts, staff counts, pending approvals, and recent activity
- **REST API**: A complete JSON API layer for every feature, with JWT authentication, so mobile apps or other clients can integrate
- **Swagger Documentation**: Interactive API docs auto-generated at `/swagger`

### Default Login Credentials

| Username | Password | Role | Access |
|----------|----------|------|--------|
| `admin` | `admin123` | Admin | Everything — dashboard, appointments, staff, users, roles, menus |

> Staff accounts are created through the registration page and require admin approval before they can log in.

---

## 2. Team Responsibilities

### Ananta — Authentication & Security Layer

| Area | What Was Built |
|------|---------------|
| Cookie Authentication | Configured in `Program.cs`, handles browser sessions |
| JWT Authentication | Token-based auth for API consumers (Postman, mobile apps) |
| Google OAuth | External login via Google accounts |
| Login/Logout | `AccountController.cs` — form-based login with validation |
| Registration | Staff self-registration with admin approval workflow |
| Password Hashing | SHA-256 hashing in `AccountController.HashPassword()` |
| Authorization Policies | Admin policy, default policy supporting dual auth schemes |
| Role-Based Access | `[Authorize]` and `[Authorize(Roles = "Admin")]` on controllers |
| CSRF Protection | `[ValidateAntiForgeryToken]` on all POST actions |
| User Management | `UsersController.cs` — admin CRUD for user accounts with role/menu assignment |
| Role Management | `RolesController.cs` — admin CRUD for roles |
| Menu Management | `MenusController.cs` — admin CRUD for navigation items |
| Auth API | `AuthApiController.cs` — login (returns JWT), logout, `/me` endpoint |
| Users API | `UsersApiController.cs` — full user CRUD + approve/reject via API |
| Roles API | `RolesApiController.cs` — full role CRUD via API |
| Menus API | `MenusApiController.cs` — full menu CRUD via API |
| Login/Register Views | `Views/Account/Login.cshtml`, `Register.cshtml`, `AccessDenied.cshtml`, `RegistrationPending.cshtml` |
| Dynamic Navigation | `MenuLoaderFilter.cs` — loads per-user menus on every request |

### Prashant — Appointment Management Module & REST API

| Area | What Was Built |
|------|---------------|
| Appointment Model | `Models/Appointment.cs` — entity with client info, timing, status, audit fields |
| Appointment Config | `Data/AppointmentConfiguration.cs` — Fluent API DB configuration |
| Appointment Controller | `Controllers/AppointmentsController.cs` — full MVC CRUD |
| Appointment API | `Controllers/Api/AppointmentsApiController.cs` — RESTful JSON API CRUD |
| Appointment Views | `Views/Appointments/` — Index, Create, Edit, Details, Delete pages |
| API DTOs | `Models/ApiDtos.cs` — Data Transfer Objects for all API endpoints |
| Swagger/OpenAPI | Configured in `Program.cs` with JWT bearer support |
| Database Seeding | `Data/DbSeeder.cs` — seeds roles, menus, and superadmin on first run |
| Postman Collection | `AppointmentSystem.postman_collection.json` for testing all API endpoints |
| Program.cs Setup | Main application configuration — all services, middleware, pipeline |
| Project Configuration | `.csproj`, `appsettings.json`, `launchSettings.json` |

### Anupam — Dashboard, Staff & UI Design System

| Area | What Was Built |
|------|---------------|
| Dashboard Controller | `Controllers/DashboardController.cs` — aggregates all system statistics |
| Dashboard API | `Controllers/Api/DashboardApiController.cs` — statistics via JSON API |
| Dashboard View | `Views/Dashboard/Index.cshtml` — stat cards, recent appointments table, quick actions |
| Dashboard ViewModel | `Models/DashboardViewModel.cs` — carries stats to the view |
| Staff Model | `Models/Staff.cs` — entity with name, email, phone, specialty |
| Staff Config | `Data/StaffConfiguration.cs` — Fluent API DB configuration |
| Staff Controller | `Controllers/StaffsController.cs` — full MVC CRUD with soft delete |
| Staff API | `Controllers/Api/StaffsApiController.cs` — RESTful JSON API CRUD |
| Staff Views | `Views/Staffs/` — Index, Create, Edit, Details, Delete pages |
| Landing Page | `Views/Home/Index.cshtml` — 774-line marketing page (hero, features, industries, integrations, CTA, footer) |
| Layout Template | `Views/Shared/_Layout.cshtml` — sidebar navigation with SVG icons, mobile responsiveness |
| CSS Design System | `wwwroot/css/site.css` — 2,845-line custom design system (no Bootstrap/Tailwind) |
| JavaScript | `wwwroot/js/site.js` — sidebar toggle, mobile nav, scroll reveal animations |
| Home Controller | `Controllers/HomeController.cs` — landing page and privacy, redirects logged-in users to dashboard |

---

## 3. Tech Stack & Architecture

### Technology Choices

| Component | Technology | Why We Chose It |
|-----------|-----------|----------------|
| **Backend Framework** | ASP.NET Core MVC (.NET 10) | Course requirement. Industry-standard C# web framework |
| **ORM** | Entity Framework Core 10 | Maps C# classes to database tables automatically. No raw SQL needed |
| **Database** | SQLite | Lightweight, file-based — no installation of MySQL/PostgreSQL required |
| **Authentication** | Cookie Auth + JWT + Google OAuth | Cookie for browser, JWT for API clients, Google for social login |
| **Frontend** | Vanilla CSS + JS | No framework dependencies (no Bootstrap, no Tailwind, no React) |
| **Fonts** | Fraunces (serif) + Inter (sans-serif) | Professional typography pairing via Google Fonts |
| **Icons** | Inline SVGs | No icon libraries needed. Lucide-style hand-coded SVGs |
| **API Docs** | Swagger/OpenAPI (Swashbuckle) | Auto-generates interactive API documentation |

### Architecture Pattern: MVC (Model-View-Controller)

```
┌─────────────────────────────────────────────────────┐
│                    User's Browser                    │
│  (Sends HTTP requests, receives HTML/JSON responses) │
└──────────────┬──────────────────────┬────────────────┘
               │ HTML Pages           │ JSON API
               ▼                      ▼
┌──────────────────────┐   ┌─────────────────────────┐
│   MVC Controllers     │   │    API Controllers       │
│   (Return Views)      │   │    (Return JSON)         │
│                       │   │                          │
│  AppointmentsController│   │  AppointmentsApiController│
│  StaffsController      │   │  StaffsApiController     │
│  DashboardController   │   │  DashboardApiController  │
│  AccountController     │   │  AuthApiController       │
│  UsersController       │   │  UsersApiController      │
│  RolesController       │   │  RolesApiController      │
│  MenusController       │   │  MenusApiController      │
└──────────┬───────────┘   └────────────┬──────────────┘
           │                             │
           ▼                             ▼
┌─────────────────────────────────────────────────────┐
│              Models (C# Classes)                     │
│   Appointment, Staff, User, Role, Menu               │
│   UserRole (junction), UserMenu (junction)           │
│   DTOs: AppointmentDto, StaffDto, LoginDto, etc.     │
└──────────────────────┬──────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────┐
│         ApplicationDbContext (EF Core)               │
│   Translates C# operations → SQL queries             │
│   Configuration files define table structure          │
└──────────────────────┬──────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────┐
│              SQLite Database File                     │
│   .db/appointment_system.db                          │
│   Tables: Staffs, Appointments, Users, Roles,        │
│           UserRoles, Menus, UserMenus                │
└─────────────────────────────────────────────────────┘
```

### How an HTTP Request Flows Through the App

1. **User clicks a link** (e.g., `/Appointments/Create`)
2. **Routing** (`Program.cs` → `MapControllerRoute`) maps the URL to `AppointmentsController.Create()`
3. **MenuLoaderFilter** runs first — loads the user's assigned menus from the database into `ViewData`
4. **Authentication middleware** verifies the user's cookie/JWT token
5. **Authorization middleware** checks if the user has the required role
6. **Controller action** runs — queries the database via `ApplicationDbContext`, builds a model
7. **View** (`.cshtml` file) receives the model and renders HTML using Razor syntax
8. **Layout** (`_Layout.cshtml`) wraps the view with the sidebar, header, and scripts
9. **Response** is sent back to the browser

---

## 4. Environment Setup & Commands

### Prerequisites

- **.NET 10 SDK** — Download from https://dotnet.microsoft.com/download/dotnet/10.0
- **Git** — For cloning the repository
- A **code editor** (VS Code or Visual Studio)

### Step-by-Step Setup

#### 1. Verify .NET is Installed

```bash
dotnet --version
# Should show 10.x.x
```

#### 2. Clone the Repository

```bash
git clone https://github.com/<your-repo-url>.git
cd Appointment-Management-System
```

#### 3. Restore NuGet Packages

When you run `dotnet run`, packages are restored automatically. But you can also do it explicitly:

```bash
dotnet restore
```

This downloads the following packages (defined in `AppointmentSystem.Web.csproj`):

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.EntityFrameworkCore` | 10.0.3 | ORM — maps C# objects to database |
| `Microsoft.EntityFrameworkCore.Sqlite` | 10.0.3 | SQLite database provider for EF Core |
| `Microsoft.EntityFrameworkCore.Tools` | 10.0.3 | CLI tools for migrations (`dotnet ef`) |
| `Microsoft.AspNetCore.Authentication.Google` | 10.0.3 | Google OAuth authentication |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.3 | JWT token authentication for APIs |
| `Swashbuckle.AspNetCore` | 10.1.3 | Swagger/OpenAPI documentation generator |

#### 4. Run the Application

```bash
dotnet run
```

The app starts on **http://localhost:5090**. On first launch:
- The SQLite database file is created at `.db/appointment_system.db` (one directory above the project)
- EF Core automatically applies migrations (creates all tables)
- `DbSeeder` seeds the default admin account, roles (Admin, Staff), and menu items

#### 5. Access the Application

| URL | What You See |
|-----|-------------|
| `http://localhost:5090` | Landing page (public marketing site) |
| `http://localhost:5090/Account/Login` | Login form |
| `http://localhost:5090/Dashboard` | Dashboard (requires login) |
| `http://localhost:5090/swagger` | Interactive API documentation |

### Database Migration Commands

We used Entity Framework Core migrations to manage the database schema. Here's what was done:

#### Installing the EF Core CLI Tools

```bash
dotnet tool install --global dotnet-ef
```

#### Creating the Initial Migration

This command was run once to generate the migration files from our model classes:

```bash
dotnet ef migrations add InitialCreate
```

This analyzed all our entity classes (`Appointment`, `Staff`, `User`, `Role`, `Menu`, `UserRole`, `UserMenu`) and their configurations, then generated:
- `Migrations/20260215135242_InitialCreate.cs` — Contains `Up()` (create tables) and `Down()` (drop tables) methods
- `Migrations/20260215135242_InitialCreate.Designer.cs` — Snapshot metadata
- `Migrations/ApplicationDbContextModelSnapshot.cs` — Current model state

#### Applying Migrations

In our case, migrations are applied **automatically** when the app starts, via this code in `Program.cs`:

```csharp
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync(); // Applies all pending migrations
    await DbSeeder.SeedAsync(context);     // Seeds default data
}
```

If you wanted to apply migrations manually instead:

```bash
dotnet ef database update
```

#### Viewing the Database

Since we use SQLite, the database is just a file. You can inspect it with:

```bash
# Using the SQLite CLI
sqlite3 .db/appointment_system.db
.tables                          # List all tables
SELECT * FROM Users;             # View users
SELECT * FROM Appointments;      # View appointments
.quit                            # Exit
```

Or use a GUI tool like **DB Browser for SQLite**.

### Installing NuGet Packages (How We Added Dependencies)

Each package was added using the `dotnet add package` command:

```bash
# Entity Framework Core (ORM)
dotnet add package Microsoft.EntityFrameworkCore --version 10.0.3

# SQLite database provider
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 10.0.3

# EF Core CLI tools (for migrations)
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 10.0.3

# Google OAuth authentication
dotnet add package Microsoft.AspNetCore.Authentication.Google --version 10.0.3

# JWT Bearer token authentication
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 10.0.3

# Swagger/OpenAPI documentation
dotnet add package Swashbuckle.AspNetCore --version 10.1.3
```

---

## 5. Project Structure Explained

```
Appointment-Management-System/
│
├── Program.cs                          ← App entry point. Configures ALL services and middleware
├── AppointmentSystem.Web.csproj        ← Project file. Lists NuGet packages and .NET version
├── appsettings.json                    ← Configuration: DB connection string, Google OAuth keys, JWT settings
├── appsettings.Development.json        ← Dev-only config overrides (just logging here)
│
├── Models/                             ← Data classes (entities + view models + DTOs)
│   ├── Appointment.cs                  ← Appointment entity (maps to Appointments table)
│   ├── Staff.cs                        ← Staff entity (maps to Staffs table)
│   ├── User.cs                         ← User entity (maps to Users table)
│   ├── Role.cs                         ← Role entity (maps to Roles table)
│   ├── UserRole.cs                     ← Junction table: User ↔ Role (many-to-many)
│   ├── Menu.cs                         ← Menu entity (maps to Menus table)
│   ├── UserMenu.cs                     ← Junction table: User ↔ Menu (many-to-many)
│   ├── DashboardViewModel.cs           ← Carries dashboard stats to the view
│   ├── LoginViewModel.cs               ← Login form data carrier
│   ├── RegisterViewModel.cs            ← Registration form data carrier
│   ├── UserFormViewModel.cs            ← Admin user create/edit form (with role/menu checkboxes)
│   ├── ErrorViewModel.cs               ← Error page data carrier
│   └── ApiDtos.cs                      ← All API request/response DTOs in one file
│
├── Data/                               ← Database layer (EF Core)
│   ├── ApplicationDbContext.cs         ← The main DbContext — gateway to the database
│   ├── DbSeeder.cs                     ← Seeds admin account, roles, menus on first run
│   ├── AppointmentConfiguration.cs     ← Fluent API config for Appointments table
│   ├── StaffConfiguration.cs           ← Fluent API config for Staffs table
│   ├── UserConfiguration.cs            ← Fluent API config for Users table
│   ├── RoleConfiguration.cs            ← Fluent API config for Roles table
│   ├── MenuConfiguration.cs            ← Fluent API config for Menus table
│   ├── UserRoleConfiguration.cs        ← Fluent API config for UserRoles junction table
│   └── UserMenuConfiguration.cs        ← Fluent API config for UserMenus junction table
│
├── Controllers/                        ← MVC controllers (return HTML views)
│   ├── HomeController.cs               ← Landing page, privacy, error
│   ├── AccountController.cs            ← Login, register, Google OAuth, logout
│   ├── DashboardController.cs          ← Dashboard statistics page
│   ├── AppointmentsController.cs       ← Appointment CRUD pages
│   ├── StaffsController.cs             ← Staff CRUD pages
│   ├── UsersController.cs              ← User management pages (admin only)
│   ├── RolesController.cs              ← Role management pages (admin only)
│   ├── MenusController.cs              ← Menu management pages (admin only)
│   └── Api/                            ← API controllers (return JSON)
│       ├── AuthApiController.cs        ← Login → JWT token, logout, /me
│       ├── AppointmentsApiController.cs← Appointment CRUD via JSON
│       ├── StaffsApiController.cs      ← Staff CRUD via JSON
│       ├── DashboardApiController.cs   ← Dashboard stats via JSON
│       ├── UsersApiController.cs       ← User CRUD + approve/reject via JSON
│       ├── RolesApiController.cs       ← Role CRUD via JSON
│       └── MenusApiController.cs       ← Menu CRUD via JSON
│
├── Filters/
│   └── MenuLoaderFilter.cs             ← Global filter: loads user's menus into ViewData per request
│
├── Views/                              ← Razor views (HTML templates with C# logic)
│   ├── _ViewImports.cshtml             ← Common imports for all views
│   ├── _ViewStart.cshtml               ← Sets default layout for all views
│   ├── Shared/
│   │   ├── _Layout.cshtml              ← Master layout: sidebar + content area
│   │   ├── _ValidationScriptsPartial.cshtml ← jQuery validation scripts
│   │   └── Error.cshtml                ← Error page
│   ├── Home/
│   │   ├── Index.cshtml                ← Landing page (774 lines — full marketing site)
│   │   └── Privacy.cshtml              ← Privacy policy
│   ├── Account/
│   │   ├── Login.cshtml                ← Login form with Google OAuth button
│   │   ├── Register.cshtml             ← Staff registration form
│   │   ├── RegistrationPending.cshtml  ← "Waiting for admin approval" page
│   │   └── AccessDenied.cshtml         ← 403 forbidden page
│   ├── Dashboard/
│   │   └── Index.cshtml                ← Dashboard with stat cards and recent appointments
│   ├── Appointments/
│   │   ├── Index.cshtml                ← Appointment list table
│   │   ├── Create.cshtml               ← New appointment form
│   │   ├── Edit.cshtml                 ← Edit appointment form
│   │   ├── Details.cshtml              ← Appointment detail view
│   │   └── Delete.cshtml               ← Delete confirmation page
│   ├── Staffs/                         ← Same CRUD pattern as Appointments
│   ├── Users/                          ← User management with role/menu checkboxes
│   ├── Roles/                          ← Role CRUD views
│   └── Menus/                          ← Menu CRUD views
│
├── wwwroot/                            ← Static files served directly to the browser
│   ├── css/site.css                    ← Complete design system (2,845 lines)
│   └── js/site.js                      ← Sidebar toggle, mobile nav, scroll animations
│
├── Migrations/                         ← EF Core migration files (auto-generated)
│   ├── 20260215135242_InitialCreate.cs
│   ├── 20260215135242_InitialCreate.Designer.cs
│   └── ApplicationDbContextModelSnapshot.cs
│
├── Properties/
│   └── launchSettings.json             ← Dev server ports (5090 HTTP, 7290 HTTPS)
│
└── AppointmentSystem.postman_collection.json ← Postman collection for testing APIs
```

---

## 6. Database Design & Entity Relationships

### Entity-Relationship Diagram

```
┌───────────────┐       ┌───────────────┐       ┌───────────────┐
│     Users      │       │   UserRoles    │       │     Roles      │
├───────────────┤       ├───────────────┤       ├───────────────┤
│ Id (PK, GUID) │──┐    │ Id (PK, GUID) │    ┌──│ Id (PK, GUID) │
│ FullName       │  │    │ UserId (FK)───│────┘  │ RoleName       │
│ Username (UQ)  │  └────│ RoleId (FK)───│───────│ Description    │
│ Email          │       │               │       │ IsActive       │
│ PasswordHash   │       │ UQ(UserId,    │       └───────────────┘
│ IsActive       │       │    RoleId)    │
│ IsApproved     │       └───────────────┘
│ CreatedAtUtc   │
└───────┬───────┘       ┌───────────────┐       ┌───────────────┐
        │               │   UserMenus    │       │     Menus      │
        │               ├───────────────┤       ├───────────────┤
        │               │ Id (PK, GUID) │    ┌──│ Id (PK, GUID) │
        └───────────────│ UserId (FK)───│────┘  │ MenuName       │
                        │ MenuId (FK)───│───────│ Url            │
                        │               │       │ DisplayOrder   │
                        │ UQ(UserId,    │       │ IsActive       │
                        │    MenuId)    │       └───────────────┘
                        └───────────────┘

┌───────────────┐       ┌─────────────────────┐
│    Staffs      │       │    Appointments      │
├───────────────┤       ├─────────────────────┤
│ Id (PK, GUID) │──┐    │ Id (PK, GUID)       │
│ FullName       │  │    │ StaffId (FK)────────│──┘
│ Email          │  │    │ ClientName           │
│ PhoneNumber    │  │    │ ClientEmail          │
│ Specialty      │  └────│ ClientPhone          │
│ IsActive       │       │ StartTime            │
│                │       │ DurationMinutes      │
│ (has many      │       │ Status               │
│  Appointments) │       │ Notes                │
└───────────────┘       │ CreatedAtUtc          │
                        │ UpdatedAtUtc          │
                        └─────────────────────┘
```

### Relationship Summary

| Relationship | Type | Explanation |
|-------------|------|-------------|
| User ↔ Role | Many-to-Many | A user can have multiple roles (Admin + Staff). A role can be assigned to many users. Connected through `UserRoles` junction table. |
| User ↔ Menu | Many-to-Many | A user sees only the menus assigned to them. Connected through `UserMenus` junction table. |
| Staff → Appointment | One-to-Many | One staff member can have many appointments. Each appointment belongs to exactly one staff member. |

### Why GUIDs Instead of Integer IDs?

We use `Guid` (Globally Unique Identifier) instead of auto-incrementing integers for primary keys because:
- **Globally unique**: No conflicts when merging data from different sources
- **Secure**: Can't guess the next ID by incrementing (e.g., `/Appointments/Details/2` → `/Appointments/Details/3`)
- **Generated client-side**: The app creates the ID before saving to the database

### Fluent API Configuration (How Database Rules Are Defined)

Instead of cluttering our model classes with database-specific attributes, we use **Fluent API** in separate configuration files. Example from `AppointmentConfiguration.cs`:

```csharp
public void Configure(EntityTypeBuilder<Appointment> builder)
{
    builder.HasKey(x => x.Id);                           // Primary key
    builder.Property(x => x.ClientName)
        .IsRequired()                                    // NOT NULL
        .HasColumnType("varchar(200)");                  // Column type
    builder.HasOne(x => x.Staff)                         // Relationship
        .WithMany(s => s.Appointments)
        .HasForeignKey(x => x.StaffId)
        .OnDelete(DeleteBehavior.Cascade);               // Delete behavior
}
```

### Database Indexes

| Table | Indexed Column(s) | Type | Purpose |
|-------|-------------------|------|---------|
| Users | Username | Unique | Fast login lookup, prevent duplicate usernames |
| Users | Email | Non-unique | Fast email lookups |
| Roles | RoleName | Unique | Prevent duplicate role names |
| Menus | DisplayOrder | Non-unique | Fast sorting for navigation rendering |
| Staffs | Email | Non-unique | Fast email lookups |
| Staffs | FullName | Non-unique | Fast name searches |
| UserRoles | (UserId, RoleId) | Unique composite | Prevent duplicate role assignments |
| UserMenus | (UserId, MenuId) | Unique composite | Prevent duplicate menu assignments |

---

## 7. Ananta's Module — Authentication & Security Layer

### 7.1 Authentication Overview

The app supports **three authentication methods**:

1. **Cookie Authentication** (for browser-based users)
2. **JWT Bearer Authentication** (for API consumers like Postman or mobile apps)
3. **Google OAuth** (for social login)

All three are configured in `Program.cs`:

```csharp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => { ... })       // Method 1: Browser cookies
    .AddGoogle(options => { ... })        // Method 3: Google OAuth
    .AddJwtBearer(options => { ... });    // Method 2: JWT tokens
```

### 7.2 Cookie Authentication (How Browser Login Works)

**File**: `Program.cs` (cookie config) + `Controllers/AccountController.cs` (login logic)

**How it works:**
1. User submits username and password on `/Account/Login`
2. `AccountController.Login()` looks up the user in the database
3. Password is hashed with SHA-256 and compared to the stored hash
4. If valid, a **ClaimsIdentity** is created containing the user's ID, username, full name, and roles
5. `HttpContext.SignInAsync()` creates an encrypted cookie and sends it to the browser
6. Every subsequent request includes this cookie — the server decrypts it to know who the user is
7. Cookie expires after **30 minutes** of inactivity

**Key configuration:**
```csharp
options.LoginPath = "/Account/Login";            // Where to redirect unauthenticated users
options.AccessDeniedPath = "/Account/AccessDenied"; // Where to redirect unauthorized users
options.ExpireTimeSpan = TimeSpan.FromMinutes(30);  // Session timeout
```

**Smart API handling**: For API requests (paths starting with `/api/`), the app returns `401 Unauthorized` or `403 Forbidden` JSON responses instead of redirecting to the login page:

```csharp
options.Events.OnRedirectToLogin = context =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    }
    context.Response.Redirect(context.RedirectUri);
    return Task.CompletedTask;
};
```

### 7.3 JWT (JSON Web Token) Authentication

**File**: `Program.cs` (JWT config) + `Controllers/Api/AuthApiController.cs` (token generation)

**What is JWT?** A JWT is a signed token (a long encoded string) that contains the user's claims. API consumers include this token in the `Authorization: Bearer <token>` header of every request.

**How it works:**
1. Client sends `POST /api/authapi/login` with `{ "username": "admin", "password": "admin123" }`
2. Server validates credentials and generates a JWT token signed with a secret key
3. Token contains: user ID, username, full name, roles, expiration time
4. Client includes the token in subsequent requests: `Authorization: Bearer eyJhbG...`
5. Server validates the token signature, checks expiration, and extracts the user's identity

**JWT Configuration** (`appsettings.json`):
```json
"Authentication": {
    "Jwt": {
        "Key": "Appo1nTr4-S3cUr3-K3y-2026-F3bRu4Ry-X9kL2mN7pQ",
        "Issuer": "Appointra",
        "Audience": "AppointraAPI",
        "ExpiryInMinutes": 60
    }
}
```

**Token generation code** (`AuthApiController.cs`):
```csharp
private (string Token, DateTime ExpiresAt) GenerateJwtToken(List<Claim> claims)
{
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Authentication:Jwt:Key"]!));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityToken(
        issuer: _configuration["Authentication:Jwt:Issuer"],
        audience: _configuration["Authentication:Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(60),
        signingCredentials: credentials);
    return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
}
```

### 7.4 Google OAuth Authentication

**File**: `Program.cs` (Google config) + `Controllers/AccountController.cs` (callback handler)

**How it works:**
1. User clicks "Sign in with Google" on the login page
2. `AccountController.GoogleLogin()` redirects to Google's consent screen
3. User authenticates with Google and grants permission
4. Google redirects back to `AccountController.GoogleCallback()` with the user's profile info
5. The app checks if a user with that email exists:
   - **New user**: Creates an account with `IsApproved = false` (pending admin approval)
   - **Existing user**: Signs them in if approved, or shows "pending" if not yet approved

**Google OAuth Configuration** (`appsettings.json`):
```json
"Authentication": {
    "Google": {
        "ClientId": "251583873893-md9n8v...",
        "ClientSecret": "GOCSPX-H8eZN..."
    }
}
```

**How Google users are handled:**
- Username is auto-generated from the email prefix (e.g., `john.doe@gmail.com` → `john.doe`)
- No local password is set (a random hash is stored since the field is required)
- They get the **Staff** role and **Appointments** menu by default
- They must wait for admin approval, same as manual registration

### 7.5 Password Hashing

**File**: `Controllers/AccountController.cs`

Passwords are **never stored in plain text**. We use SHA-256 hashing:

```csharp
public static string HashPassword(string password)
{
    using var sha256 = System.Security.Cryptography.SHA256.Create();
    var bytes = System.Text.Encoding.UTF8.GetBytes(password);
    var hash = sha256.ComputeHash(bytes);
    return Convert.ToBase64String(hash);
}
```

**How verification works:**
```csharp
private bool VerifyPassword(string password, string storedHash)
{
    return HashPassword(password) == storedHash;
    // Hash the input password and compare to the stored hash
}
```

> **Note**: SHA-256 is used here for simplicity. In production, you'd use bcrypt or Argon2 which include salting and are designed for password hashing.

### 7.6 Authorization (Role-Based Access Control)

**How roles work in the system:**

1. **Admin role**: Full access to everything — appointments, staff, users, roles, menus
2. **Staff role**: Limited access — can view/manage appointments only

**Protected controllers:**

| Controller | Protection | Who Can Access |
|------------|-----------|---------------|
| `HomeController` | None | Everyone (public) |
| `AccountController` | None | Everyone (login/register pages) |
| `DashboardController` | `[Authorize]` | Any logged-in user |
| `AppointmentsController` | `[Authorize]` | Any logged-in user |
| `StaffsController` | `[Authorize]` on class, `[Authorize(Roles = "Admin")]` on Create/Edit/Delete | View: any user. Modify: admin only |
| `UsersController` | `[Authorize(Roles = "Admin")]` | Admin only |
| `RolesController` | `[Authorize(Roles = "Admin")]` | Admin only |
| `MenusController` | `[Authorize(Roles = "Admin")]` | Admin only |

**Authorization policies** (for API controllers):
```csharp
builder.Services.AddAuthorization(options =>
{
    // Default policy: authenticated via Cookie OR JWT
    options.DefaultPolicy = new AuthorizationPolicyBuilder(
        CookieAuthenticationDefaults.AuthenticationScheme,
        JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build();

    // Admin policy: must be authenticated AND have the Admin role
    options.AddPolicy("Admin", policy =>
        policy.AddAuthenticationSchemes(...)
            .RequireAuthenticatedUser()
            .RequireRole("Admin"));
});
```

### 7.7 CSRF Protection

Every form submission (POST request) includes a hidden anti-forgery token. This prevents **Cross-Site Request Forgery** attacks where a malicious site tricks a user's browser into submitting forms on our site.

```csharp
[HttpPost]
[ValidateAntiForgeryToken]  // ← This attribute validates the token
public async Task<IActionResult> Login(LoginViewModel model) { ... }
```

In Razor views, the `<form asp-action="...">` tag helper automatically generates the hidden token field.

### 7.8 Staff Registration & Approval Workflow

**File**: `Controllers/AccountController.cs`

The registration flow has an **approval gate**:

```
Staff clicks "Register"
        │
        ▼
Fills out the form (name, username, email, password)
        │
        ▼
Account created with IsApproved = false
Auto-assigned: Staff role + Appointments menu
        │
        ▼
Redirected to "Registration Pending" page
        │
        ▼
Admin sees the pending user in /Users (highlighted at top)
        │
        ├─── Admin clicks "Approve" → IsApproved = true → Staff can now log in
        │
        └─── Admin clicks "Reject" → User account deleted
```

### 7.9 Claims-Based Identity

When a user logs in, we create **claims** — key-value pairs that describe the user:

```csharp
var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),  // User's GUID
    new Claim(ClaimTypes.Name, user.Username),                  // Username
    new Claim("FullName", user.FullName)                        // Display name
};

// Add a claim for each role
foreach (var userRole in user.UserRoles)
{
    claims.Add(new Claim(ClaimTypes.Role, userRole.Role.RoleName));
}
```

These claims are stored in the cookie (or JWT token) and are available throughout the app via `User.Claims`.

### 7.10 Dynamic Navigation (MenuLoaderFilter)

**File**: `Filters/MenuLoaderFilter.cs`

This is a **global action filter** that runs before every controller action:

1. Checks if the user is authenticated
2. Reads the user's ID from their claims
3. Queries the `UserMenus` table to find which menus are assigned to this user
4. Stores the result in `ViewData["UserMenus"]`
5. The `_Layout.cshtml` reads `ViewData["UserMenus"]` to render the sidebar navigation

**Why this matters**: Different users see different sidebar menus. An admin might see Appointments, Staff, Users, Roles, Menus. A staff member might only see Appointments.

### 7.11 User Management (Admin Panel)

**File**: `Controllers/UsersController.cs`

The admin can:
- **View all users**: Sorted with pending approvals at the top
- **Approve/Reject**: Pending registrations have approve/reject buttons
- **Create users**: Form with role checkboxes and menu checkboxes
- **Edit users**: Update name, username, email, password, active status, roles, and menus
- **Delete users**: Removes the user and all their role/menu assignments

**UserFormViewModel** provides checkboxes for roles and menus:
```csharp
public class UserFormViewModel
{
    // Basic user fields
    public string FullName { get; set; }
    public string Username { get; set; }
    // ...

    // Checkboxes populated from the database
    public List<RoleAssignment> Roles { get; set; }   // Each has RoleId, RoleName, IsSelected
    public List<MenuAssignment> Menus { get; set; }   // Each has MenuId, MenuName, IsSelected
}
```

---

## 8. Prashant's Module — Appointment Management & REST API

### 8.1 Appointment Model

**File**: `Models/Appointment.cs`

The `Appointment` class represents a booking in the system:

```csharp
public class Appointment
{
    public Guid Id { get; set; }                    // Unique identifier

    // Who is providing the service
    [Required]
    public Guid StaffId { get; set; }               // Foreign key to Staff
    public Staff? Staff { get; set; }               // Navigation property

    // Client information
    [Required, StringLength(200)]
    public string ClientName { get; set; }
    [EmailAddress, StringLength(255)]
    public string? ClientEmail { get; set; }         // Optional
    [Required, Phone, StringLength(20)]
    public string ClientPhone { get; set; }

    // Timing
    [Required]
    public DateTime StartTime { get; set; }
    [Required, Range(1, 1440)]
    public int DurationMinutes { get; set; }         // 1 min to 24 hours

    // Status tracking
    [Required, StringLength(50)]
    public string Status { get; set; } = "Scheduled"; // Default status
    [StringLength(500)]
    public string? Notes { get; set; }               // Optional notes

    // Audit trail
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }      // Null until first edit
}
```

**Data annotations explained:**
- `[Required]` — Field must have a value (becomes NOT NULL in the database)
- `[StringLength(200)]` — Maximum character count
- `[EmailAddress]` — Validates email format (e.g., must contain @)
- `[Phone]` — Validates phone number format
- `[Range(1, 1440)]` — Value must be between 1 and 1440

### 8.2 Appointment MVC Controller (Web Pages)

**File**: `Controllers/AppointmentsController.cs`

This controller provides **CRUD operations** through web pages:

| URL | HTTP Method | Action | What It Does |
|-----|-------------|--------|-------------|
| `/Appointments` | GET | `Index()` | Shows all appointments in a table |
| `/Appointments/Details/{id}` | GET | `Details(id)` | Shows one appointment's full details |
| `/Appointments/Create` | GET | `Create()` | Shows the booking form |
| `/Appointments/Create` | POST | `Create(appointment)` | Saves a new appointment |
| `/Appointments/Edit/{id}` | GET | `Edit(id)` | Shows the edit form with current data |
| `/Appointments/Edit/{id}` | POST | `Edit(id, appointment)` | Saves the updated appointment |
| `/Appointments/Delete/{id}` | GET | `Delete(id)` | Shows delete confirmation page |
| `/Appointments/Delete/{id}` | POST | `DeleteConfirmed(id)` | Actually deletes the appointment |

**Key patterns used:**

1. **Eager Loading** (`Include`): When loading appointments, we also load the staff member info:
   ```csharp
   var appointments = await _context.Appointments
       .Include(a => a.Staff)         // Load staff data along with each appointment
       .OrderByDescending(a => a.StartTime)
       .ToListAsync();
   ```

2. **Dropdown Population** (`SelectList`): Staff members are shown in a dropdown on the create/edit forms:
   ```csharp
   ViewData["StaffId"] = new SelectList(_context.Staffs, "Id", "FullName");
   ```

3. **Concurrency Handling**: When saving edits, we catch `DbUpdateConcurrencyException` in case someone else deleted the record while we were editing it.

4. **Anti-forgery tokens**: Every POST action has `[ValidateAntiForgeryToken]` for security.

5. **Model Binding** (`[Bind]`): Specifies exactly which form fields to accept, preventing over-posting attacks:
   ```csharp
   public async Task<IActionResult> Create(
       [Bind("Id,StaffId,ClientName,ClientEmail,ClientPhone,StartTime,DurationMinutes,Status,Notes")]
       Appointment appointment)
   ```

### 8.3 Appointment REST API

**File**: `Controllers/Api/AppointmentsApiController.cs`

The API provides the same CRUD operations but returns JSON instead of HTML:

| URL | HTTP Method | What It Does | Auth Required |
|-----|-------------|-------------|---------------|
| `GET /api/appointmentsapi` | GET | Returns all appointments | Yes (any user) |
| `GET /api/appointmentsapi/{id}` | GET | Returns one appointment | Yes (any user) |
| `POST /api/appointmentsapi` | POST | Creates an appointment | Yes (any user) |
| `PUT /api/appointmentsapi/{id}` | PUT | Updates an appointment | Yes (any user) |
| `DELETE /api/appointmentsapi/{id}` | DELETE | Deletes an appointment | Yes (any user) |

**DTOs (Data Transfer Objects)**: Instead of sending the raw entity to the API client, we use DTOs to control exactly what data is sent and received:

```csharp
// What the API returns (response)
public class AppointmentDto
{
    public Guid Id { get; set; }
    public Guid StaffId { get; set; }
    public string StaffName { get; set; }     // ← Resolved from Staff entity
    public string ClientName { get; set; }
    // ... all other fields
}

// What the API accepts (request)
public class AppointmentCreateDto
{
    [Required]
    public Guid StaffId { get; set; }
    [Required, StringLength(200)]
    public string ClientName { get; set; }
    // ... all other fields
}
```

**Why DTOs?** They separate the internal database model from the external API contract. For example, `AppointmentDto` includes `StaffName` (a convenient string) while the database only stores `StaffId` (a GUID).

**API response examples:**

```json
// GET /api/appointmentsapi
[
  {
    "id": "a1b2c3d4-...",
    "staffId": "e5f6g7h8-...",
    "staffName": "Dr. Smith",
    "clientName": "John Doe",
    "clientEmail": "john@example.com",
    "clientPhone": "555-0123",
    "startTime": "2026-02-25T10:00:00",
    "durationMinutes": 30,
    "status": "Scheduled",
    "notes": "First visit",
    "createdAtUtc": "2026-02-20T08:30:00Z"
  }
]
```

### 8.4 API DTOs (Data Transfer Objects)

**File**: `Models/ApiDtos.cs`

All API DTOs are organized in a single file, grouped by feature:

| DTO | Purpose |
|-----|---------|
| `AppointmentDto` | API response for appointment data |
| `AppointmentCreateDto` | API request body for creating/updating appointments |
| `StaffDto` | API response for staff data (includes appointment count) |
| `StaffCreateDto` | API request body for creating/updating staff |
| `UserDto` | API response for user data (includes role and menu names) |
| `UserCreateDto` | API request body for creating users (includes role/menu IDs) |
| `RoleDto` | API response for role data (includes user count) |
| `RoleCreateDto` | API request body for creating/updating roles |
| `MenuDto` | API response for menu data |
| `MenuCreateDto` | API request body for creating/updating menus |
| `LoginDto` | API request body for login |
| `LoginResponseDto` | API response after login (includes JWT token) |
| `DashboardDto` | API response for dashboard statistics |

### 8.5 Swagger/OpenAPI Configuration

**File**: `Program.cs`

Swagger auto-generates interactive API documentation at `/swagger`:

```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Appointra API",
        Version = "v1",
        Description = "REST API for the Appointra Appointment Management System."
    });

    // Adds an "Authorize" button in Swagger UI for JWT tokens
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token."
    });
});
```

**How to use Swagger:**
1. Go to `http://localhost:5090/swagger`
2. Click "Authorize" button
3. Paste your JWT token (obtained from `POST /api/authapi/login`)
4. Now you can test any API endpoint directly from the browser

### 8.6 Database Seeding

**File**: `Data/DbSeeder.cs`

On first app launch (when tables are empty), `DbSeeder.SeedAsync()` creates:

1. **Two roles**: `Admin` and `Staff`
2. **Five menus**: Appointments, Staff, Users, Roles, Menus
3. **One superadmin user**: Username `admin`, password `admin123`
   - Auto-approved (`IsApproved = true`)
   - Assigned the Admin role
   - Assigned all five menus

**Seeding happens automatically** in `Program.cs`:
```csharp
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();
    await DbSeeder.SeedAsync(context);
}
```

### 8.7 Program.cs — The Application Configuration Hub

`Program.cs` is the single file where everything is configured. Here's the order:

1. **Service Registration** (what the app needs):
   - MVC with `MenuLoaderFilter`
   - Swagger/OpenAPI
   - `MenuLoaderFilter` as scoped service
   - `ApplicationDbContext` with SQLite
   - Cookie authentication
   - Google OAuth
   - JWT Bearer authentication
   - Authorization policies

2. **App Building** (build the configured app)

3. **Database Migration & Seeding** (run on startup)

4. **Middleware Pipeline** (order matters!):
   - Swagger UI
   - Exception handler (production only)
   - HTTPS redirection
   - Static files (`wwwroot/`)
   - Routing
   - Authentication (WHO are you?)
   - Authorization (WHAT can you do?)
   - Controller route mapping

---

## 9. Anupam's Module — Dashboard, Staff & UI Design System

### 9.1 Dashboard

**Files**: `Controllers/DashboardController.cs`, `Models/DashboardViewModel.cs`, `Views/Dashboard/Index.cshtml`

The dashboard is the first page users see after logging in. It provides a bird's-eye view of the system.

**DashboardController** queries the database for statistics:

```csharp
var viewModel = new DashboardViewModel
{
    TotalAppointments = await _context.Appointments.CountAsync(),
    ScheduledAppointments = await _context.Appointments.CountAsync(a => a.Status == "Scheduled"),
    CompletedAppointments = await _context.Appointments.CountAsync(a => a.Status == "Completed"),
    CancelledAppointments = await _context.Appointments.CountAsync(a => a.Status == "Cancelled"),
    TodayAppointments = await _context.Appointments.CountAsync(a => a.StartTime.Date == today),
    TotalStaff = await _context.Staffs.CountAsync(),
    ActiveStaff = await _context.Staffs.CountAsync(s => s.IsActive),
    TotalUsers = await _context.Users.CountAsync(),
    ActiveUsers = await _context.Users.CountAsync(u => u.IsActive),
    PendingApprovals = await _context.Users.CountAsync(u => !u.IsApproved),
    RecentAppointments = await _context.Appointments
        .Include(a => a.Staff)
        .OrderByDescending(a => a.CreatedAtUtc)
        .Take(5)
        .ToListAsync()
};
```

**Dashboard View features:**
- **Time-aware greeting**: "Good morning/afternoon/evening, [FirstName]"
- **Current date display**: Shows full date like "Tuesday, February 25, 2026"
- **Pending approvals banner**: Alert card when staff registrations need review
- **Quick action buttons**: New Appointment, Add Staff, View Calendar
- **Stat cards grid**: 6 cards showing key metrics with color-coded values
- **Recent appointments table**: Last 5 appointments with status badges
- **Empty state**: Friendly message when no appointments exist yet

### 9.2 Staff Management

**Files**: `Models/Staff.cs`, `Controllers/StaffsController.cs`, `Views/Staffs/`

**Staff Model:**
```csharp
public class Staff
{
    public Guid Id { get; set; }
    public string FullName { get; set; }       // Required, max 100 chars
    public string? Email { get; set; }          // Optional
    public string? PhoneNumber { get; set; }    // Optional
    public string? Specialty { get; set; }      // e.g., "Pediatrics", "Physical Therapy"
    public bool IsActive { get; set; } = true;  // Soft-delete flag
    public List<Appointment> Appointments { get; set; } = new(); // Navigation
}
```

**Staff Controller CRUD:**

| URL | Method | Action | Access |
|-----|--------|--------|--------|
| `/Staffs` | GET | `Index()` | Any authenticated user |
| `/Staffs/Details/{id}` | GET | `Details(id)` | Any authenticated user |
| `/Staffs/Create` | GET/POST | `Create()` | Admin only |
| `/Staffs/Edit/{id}` | GET/POST | `Edit(id)` | Admin only |
| `/Staffs/Delete/{id}` | GET/POST | `Delete(id)` | Admin only |

**Smart Delete Logic:**
The staff controller uses **soft delete** when a staff member has existing appointments:

```csharp
if (staff.Appointments.Any())
{
    // Has appointments → just deactivate (preserve historical data)
    staff.IsActive = false;
    _context.Staffs.Update(staff);
}
else
{
    // No appointments → safe to permanently delete
    _context.Staffs.Remove(staff);
}
```

This preserves data integrity — we don't want to delete a staff member and lose all their appointment history.

### 9.3 Staff REST API

**File**: `Controllers/Api/StaffsApiController.cs`

| URL | Method | What It Does | Auth |
|-----|--------|-------------|------|
| `GET /api/staffsapi` | GET | All staff with appointment counts | Any user |
| `GET /api/staffsapi/{id}` | GET | Single staff member | Any user |
| `POST /api/staffsapi` | POST | Create staff member | Admin only |
| `PUT /api/staffsapi/{id}` | PUT | Update staff member | Admin only |
| `DELETE /api/staffsapi/{id}` | DELETE | Delete/deactivate staff | Admin only |

### 9.4 UI Design System

**File**: `wwwroot/css/site.css` (2,845 lines)

The entire frontend is built with a **custom CSS design system** — no Bootstrap, no Tailwind, no CSS framework. Everything is hand-written.

#### Design Tokens (CSS Custom Properties)

All design values are defined as CSS variables in `:root`:

```css
:root {
  /* Colors */
  --color-bg:           #f7f4ee;     /* Warm cream background */
  --color-surface:      #ffffff;     /* Card/panel backgrounds */
  --color-primary:      #1a3a34;     /* Deep forest green */
  --color-accent:       #2d6a4f;     /* Action/link green */
  --color-danger:       #c0392b;     /* Red for errors/cancel */
  --color-warning:      #e2a03f;     /* Yellow for warnings */
  --color-success:      #27ae60;     /* Green for success */
  --color-info:         #2980b9;     /* Blue for info */

  /* Typography */
  --font-sans:  'Inter', sans-serif;           /* Body text */
  --font-serif: 'Fraunces', serif;             /* Headings and brand */

  /* Spacing */
  --space-xs: 0.25rem;  --space-sm: 0.5rem;   --space-md: 1rem;
  --space-lg: 1.5rem;   --space-xl: 2rem;     --space-2xl: 3rem;

  /* Borders */
  --radius-sm: 6px;  --radius-md: 10px;  --radius-lg: 16px;

  /* Shadows */
  --shadow-sm: 0 1px 2px rgba(0,0,0,.04);
  --shadow-md: 0 4px 16px rgba(0,0,0,.06);

  /* Layout */
  --sidebar-width: 260px;
}
```

#### Key UI Components

| Component | CSS Class | Where Used |
|-----------|-----------|-----------|
| Sidebar navigation | `.sidebar`, `.sidebar-nav` | Layout (authenticated pages) |
| Stat cards | `.stat-card`, `.stat-value` | Dashboard |
| Data tables | `.table-custom` | Appointments, Staff, Users, Roles, Menus lists |
| Form inputs | `.form-input`, `.form-group` | All create/edit forms |
| Buttons | `.btn-primary-custom`, `.btn-secondary-custom`, `.btn-danger-custom` | Everywhere |
| Status badges | `.badge-status`, `.badge-scheduled`, `.badge-completed` | Appointment tables |
| Cards | `.card-custom` | Dashboard, detail views |
| Empty states | `.empty-state` | When no data exists |
| Login card | `.login-card`, `.login-page` | Login, Register pages |
| Quick actions | `.quick-action` | Dashboard |
| Landing page | `.ln-hero`, `.ln-feature-card`, `.ln-nav` | Home page |

#### Responsive Design

The design has three breakpoints:

```css
@media (max-width: 992px)  { /* Tablet: sidebar collapses to overlay */ }
@media (max-width: 768px)  { /* Mobile: stack columns, smaller text */ }
@media (max-width: 480px)  { /* Small mobile: further adjustments */ }
```

On mobile devices:
- The sidebar hides behind a hamburger menu (☰)
- A translucent overlay appears behind the sidebar
- Tables get horizontal scroll
- Stat cards stack vertically

#### Accessibility Features

```css
/* Focus outlines for keyboard navigation */
:focus-visible { outline: 2px solid var(--color-accent); outline-offset: 2px; }

/* Reduced motion support */
@media (prefers-reduced-motion: reduce) {
    *, *::before, *::after { animation-duration: 0.01ms !important; }
}
```

### 9.5 Landing Page

**File**: `Views/Home/Index.cshtml` (774 lines)

The landing page is a complete marketing website with these sections:
- **Navigation bar**: Logo, anchor links, login/CTA buttons
- **Hero section**: Split layout with headline + CSS-built dashboard mockup
- **Social proof**: Trust badges and stats
- **Features grid**: 6 feature cards with icons
- **Detail rows**: Feature deep-dives with expandable accordions
- **Industries section**: Cards for healthcare, beauty, fitness, professional services
- **Integrations bar**: Brand logos of integrations
- **CTA section**: Final call-to-action
- **Footer**: Links, copyright, social icons

All built with CSS — no images except Unsplash photos loaded from CDN.

### 9.6 Layout Template

**File**: `Views/Shared/_Layout.cshtml`

The layout has **two modes**:

1. **Authenticated mode** (logged-in users): Shows sidebar + content area
   - Sidebar brand link ("appointra")
   - Dashboard link (always visible)
   - Dynamic menu items from `ViewData["UserMenus"]` with SVG icons
   - User info card at bottom (avatar initial, full name, role)
   - Sign out button

2. **Public mode** (not logged in): Clean wrapper for landing/login pages

The layout also includes:
- jQuery + jQuery Validation (for form validation)
- Custom JavaScript (`site.js`)
- Mobile hamburger menu toggle

### 9.7 JavaScript

**File**: `wwwroot/js/site.js`

Three main features:

1. **Sidebar toggle**: Opens/closes the mobile sidebar with overlay
2. **Landing page mobile nav**: Hamburger ↔ X icon toggle
3. **Scroll reveal animations**: Uses `IntersectionObserver` to animate elements as they scroll into view

```javascript
// Elements with these classes animate when they scroll into view
var revealElements = document.querySelectorAll('.reveal, .reveal-left, .reveal-right, .reveal-scale');
var observer = new IntersectionObserver(function(entries) {
    entries.forEach(function(entry) {
        if (entry.isIntersecting) {
            entry.target.classList.add('visible');  // Triggers CSS animation
            observer.unobserve(entry.target);       // Only animate once
        }
    });
}, { threshold: 0.1, rootMargin: '0px 0px -40px 0px' });
```

---

## 10. How the Application Starts (Program.cs Walkthrough)

Here's exactly what happens when you run `dotnet run`:

### Step 1: Create the Builder
```csharp
var builder = WebApplication.CreateBuilder(args);
```
Creates a new web application builder with default configuration.

### Step 2: Register Services (Dependency Injection)

```csharp
// 1. MVC with global filter
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.AddService<MenuLoaderFilter>();
});

// 2. Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => { ... });

// 3. MenuLoaderFilter
builder.Services.AddScoped<MenuLoaderFilter>();

// 4. Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("AppointmentSystem")));

// 5. Authentication (Cookie + Google + JWT)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => { ... })
    .AddGoogle(options => { ... })
    .AddJwtBearer(options => { ... });

// 6. Authorization policies
builder.Services.AddAuthorization(options => { ... });
```

### Step 3: Build the App
```csharp
var app = builder.Build();
```

### Step 4: Database Migration & Seeding
```csharp
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();     // Create/update tables
    await DbSeeder.SeedAsync(context);          // Seed default data
}
```

### Step 5: Configure Middleware Pipeline (order matters!)
```csharp
app.UseSwagger();                    // 1. Swagger JSON endpoint
app.UseSwaggerUI();                  // 2. Swagger UI page
app.UseExceptionHandler("/Home/Error"); // 3. Error handling (prod only)
app.UseHsts();                       // 4. HTTP Strict Transport Security (prod only)
app.UseHttpsRedirection();           // 5. HTTP → HTTPS redirect
app.UseStaticFiles();                // 6. Serve CSS, JS from wwwroot/
app.UseRouting();                    // 7. Enable URL routing
app.UseAuthentication();             // 8. WHO are you? (read cookie/JWT)
app.UseAuthorization();              // 9. WHAT can you do? (check roles)
app.MapControllerRoute(...);         // 10. Map URLs to controller actions
await app.RunAsync();                // 11. Start listening!
```

> **The order of middleware matters!** Authentication must come before Authorization. Routing must come before both. Static files are served before auth checks (CSS/JS don't need login).

---

## 11. API Documentation & Testing

### Complete API Endpoint Reference

#### Authentication API (`/api/authapi`)

| Method | Endpoint | Body | Response | Auth |
|--------|----------|------|----------|------|
| POST | `/api/authapi/login` | `{ "username": "admin", "password": "admin123" }` | JWT token + user info | No |
| POST | `/api/authapi/logout` | — | Success message | Yes |
| GET | `/api/authapi/me` | — | Current user info | Yes |

#### Appointments API (`/api/appointmentsapi`)

| Method | Endpoint | Body | Response | Auth |
|--------|----------|------|----------|------|
| GET | `/api/appointmentsapi` | — | Array of appointments | Yes |
| GET | `/api/appointmentsapi/{id}` | — | Single appointment | Yes |
| POST | `/api/appointmentsapi` | `AppointmentCreateDto` | Created appointment | Yes |
| PUT | `/api/appointmentsapi/{id}` | `AppointmentCreateDto` | Updated appointment | Yes |
| DELETE | `/api/appointmentsapi/{id}` | — | 204 No Content | Yes |

#### Staff API (`/api/staffsapi`)

| Method | Endpoint | Body | Response | Auth |
|--------|----------|------|----------|------|
| GET | `/api/staffsapi` | — | Array of staff | Yes |
| GET | `/api/staffsapi/{id}` | — | Single staff | Yes |
| POST | `/api/staffsapi` | `StaffCreateDto` | Created staff | Admin |
| PUT | `/api/staffsapi/{id}` | `StaffCreateDto` | Updated staff | Admin |
| DELETE | `/api/staffsapi/{id}` | — | 204 or soft-delete message | Admin |

#### Users API (`/api/usersapi`) — Admin Only

| Method | Endpoint | Body | Response | Auth |
|--------|----------|------|----------|------|
| GET | `/api/usersapi` | — | Array of users | Admin |
| GET | `/api/usersapi/{id}` | — | Single user | Admin |
| POST | `/api/usersapi` | `UserCreateDto` | Created user | Admin |
| POST | `/api/usersapi/{id}/approve` | — | Approval message | Admin |
| POST | `/api/usersapi/{id}/reject` | — | 204 No Content | Admin |
| DELETE | `/api/usersapi/{id}` | — | 204 No Content | Admin |

#### Roles API (`/api/rolesapi`) — Admin Only

| Method | Endpoint | Body | Response | Auth |
|--------|----------|------|----------|------|
| GET | `/api/rolesapi` | — | Array of roles | Admin |
| GET | `/api/rolesapi/{id}` | — | Single role | Admin |
| POST | `/api/rolesapi` | `RoleCreateDto` | Created role | Admin |
| PUT | `/api/rolesapi/{id}` | `RoleCreateDto` | Updated role | Admin |
| DELETE | `/api/rolesapi/{id}` | — | 204 No Content | Admin |

#### Menus API (`/api/menusapi`) — Admin Only

| Method | Endpoint | Body | Response | Auth |
|--------|----------|------|----------|------|
| GET | `/api/menusapi` | — | Array of menus | Admin |
| GET | `/api/menusapi/{id}` | — | Single menu | Admin |
| POST | `/api/menusapi` | `MenuCreateDto` | Created menu | Admin |
| PUT | `/api/menusapi/{id}` | `MenuCreateDto` | Updated menu | Admin |
| DELETE | `/api/menusapi/{id}` | — | 204 No Content | Admin |

#### Dashboard API (`/api/dashboardapi`)

| Method | Endpoint | Body | Response | Auth |
|--------|----------|------|----------|------|
| GET | `/api/dashboardapi` | — | Dashboard statistics | Yes |

### Testing with Postman

A Postman collection (`AppointmentSystem.postman_collection.json`) is included in the project root. Import it into Postman to test all endpoints.

**Steps to test:**
1. Import the collection into Postman
2. Send `POST /api/authapi/login` with admin credentials
3. Copy the `token` from the response
4. Set the `Authorization` header to `Bearer <token>` for all subsequent requests
5. Test any endpoint

### Testing with Swagger UI

1. Navigate to `http://localhost:5090/swagger`
2. Click the **Authorize** button (lock icon)
3. First, log in: Find `POST /api/authapi/login`, click "Try it out", enter credentials
4. Copy the `token` from the response
5. Click **Authorize** again, paste the token, click "Authorize"
6. Now test any endpoint — Swagger will include the JWT token automatically

---

## 12. Common Cross-Questions & Answers

### Architecture & Design

**Q: Why MVC pattern instead of a SPA (like React)?**
A: MVC is a core ASP.NET Core pattern and a course requirement. It demonstrates server-side rendering, Razor views, form handling, and the full request lifecycle — all fundamental web concepts.

**Q: Why SQLite instead of SQL Server or PostgreSQL?**
A: SQLite requires zero installation — no database server setup. It's a single file. Perfect for a coursework project that needs to be easily portable and demo-ready. The code would work with SQL Server by simply changing the connection string and NuGet package.

**Q: Why GUIDs instead of auto-increment integers for IDs?**
A: GUIDs are globally unique (no collisions), can be generated client-side, and don't reveal record counts. With integers, someone could guess `/Appointments/Details/2` is the second appointment ever created.

**Q: Why separate configuration classes instead of data annotations?**
A: We use both. Data annotations (`[Required]`, `[StringLength]`) handle validation. Fluent API configuration classes handle database-specific rules (indexes, relationships, column types). This keeps the model classes clean and focused on business logic.

### Authentication & Security

**Q: Why SHA-256 for password hashing? Isn't bcrypt better?**
A: Yes, bcrypt or Argon2 would be more secure for production because they include salting and are intentionally slow (to resist brute-force attacks). We used SHA-256 for simplicity in this coursework project — it still ensures passwords are never stored in plain text.

**Q: Why both Cookie and JWT authentication?**
A: Cookie auth is for browser users (the MVC web app). JWT is for API consumers (Postman, mobile apps, other services). Having both means the same app serves both web pages and a REST API.

**Q: How does the anti-forgery token prevent CSRF?**
A: When the server renders a form, it includes a hidden token. When the form is submitted, the server verifies the token matches. A malicious site can't access this token because it's embedded in our HTML and protected by the same-origin policy.

**Q: What happens if a JWT token is stolen?**
A: JWT tokens expire after 60 minutes (configurable). There's no server-side revocation since JWTs are stateless — the expiry is the primary defense. In production, you'd add a token blacklist or use shorter expiry times with refresh tokens.

**Q: How does Google OAuth security work?**
A: We never see the user's Google password. Google handles authentication and sends us a signed token with the user's email and name. We verify the token's authenticity, then create/match a local account. The `ClientId` and `ClientSecret` are stored in `appsettings.json`.

### Database & Data Layer

**Q: What is Entity Framework Core and why did we use it?**
A: EF Core is an ORM (Object-Relational Mapper). It lets us work with the database using C# classes and LINQ queries instead of writing raw SQL. For example, `_context.Appointments.Where(a => a.Status == "Scheduled")` is translated to `SELECT * FROM Appointments WHERE Status = 'Scheduled'` automatically.

**Q: What are migrations?**
A: Migrations are version-controlled database schema changes. When we add a property to a model (e.g., adding `Notes` to `Appointment`), we run `dotnet ef migrations add AddNotes`, which generates code that alters the database table. This keeps the database schema in sync with our C# models.

**Q: What is a junction table?**
A: A junction table enables many-to-many relationships. For example, a user can have multiple roles, and a role can belong to multiple users. The `UserRoles` table has two foreign keys (`UserId` and `RoleId`), creating the link between the two entities.

**Q: Why use `Include()` when loading data?**
A: EF Core uses **lazy loading** by default — related data isn't loaded unless explicitly requested. `Include()` tells EF Core to load related entities in the same query (eager loading). Without it, `appointment.Staff` would be null.

**Q: What is cascade delete?**
A: When we delete a parent record, cascade delete automatically deletes related child records. For example, if we delete a staff member, all their appointments are deleted too. We configured this in `AppointmentConfiguration`: `OnDelete(DeleteBehavior.Cascade)`.

### Frontend & UI

**Q: Why no Bootstrap or Tailwind?**
A: We built a custom design system to demonstrate understanding of CSS fundamentals — layout, responsive design, custom properties, animations. It also means zero external dependencies and full control over the design.

**Q: How does the dynamic sidebar work?**
A: The `MenuLoaderFilter` runs before every request. It queries the `UserMenus` table to find which menus are assigned to the current user. These are stored in `ViewData["UserMenus"]`. The `_Layout.cshtml` loops through this list and renders sidebar links with SVG icons.

**Q: How do the scroll animations work?**
A: We use the `IntersectionObserver` API in JavaScript. Elements with the `.reveal` class start invisible (via CSS). When they scroll into the viewport, the observer adds a `.visible` class, which triggers a CSS transition (fade + slide up).

### API Design

**Q: Why DTOs instead of returning entities directly?**
A: DTOs (Data Transfer Objects) let us control exactly what data is sent over the API. For example, we never expose `PasswordHash` through the API. We can also include computed fields like `StaffName` (resolved from a join) or `AppointmentCount` that don't exist in the raw entity.

**Q: What does `[ApiController]` do?**
A: It enables API-specific behaviors: automatic model validation (returns 400 if invalid), automatic `[FromBody]` binding for complex types, and problem details responses for errors.

**Q: What does `[ProducesResponseType]` do?**
A: It tells Swagger what response types each endpoint can return, so the API documentation shows the correct response schema. For example:
```csharp
[ProducesResponseType(typeof(AppointmentDto), 200)]  // Success → returns AppointmentDto
[ProducesResponseType(404)]                            // Not found → no body
```

---

> **This document was created as a presentation guide for the Appointra project. Each team member should be able to explain their module in detail using the sections above.**
