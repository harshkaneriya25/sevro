# Sevro – Local Service Marketplace

A clean, database-driven **ASP.NET Core MVC (.NET 8)** web application built for BCA Third Year (TY) that connects local customers with admin-verified service providers (electricians, plumbers, cleaners, painters, mechanics, and repair specialists).

---

## Technology Stack
- **Backend**: ASP.NET Core MVC (.NET 8 / compatible with .NET 9+), C#
- **Database**: MySQL / MariaDB via **XAMPP Server** (manageable via phpMyAdmin)
- **ORM**: Entity Framework Core (`Pomelo.EntityFrameworkCore.MySql`, Code-First with Migrations)
- **Authentication**: ASP.NET Core Identity (Role-based: `Admin`, `Customer`, `Provider`)
- **Frontend**: HTML5, CSS3 (`site.css`), JavaScript (`site.js`), Bootstrap 5.3 + Bootstrap Icons
- **Custom Favicon**: Integrated from the uploaded Sevro house-and-pin logo (`wwwroot/favicon.ico` & `wwwroot/images/favicon.png`)

---

## Setup & Run Instructions (XAMPP MySQL)

1. Open **XAMPP Control Panel** and click **Start** next to **MySQL** (and **Apache** if you want to inspect tables in `http://localhost/phpmyadmin`).
2. Verify the MySQL connection string in `Sevro/appsettings.json`:
   ```json
   "DefaultConnection": "Server=localhost;Port=3306;Database=SevroDb;User=root;Password=;"
   ```
   *(Update `Password` or `Port` if your XAMPP MySQL uses custom credentials.)*
3. Apply the EF Core Code-First Migration to create `SevroDb`:
   ```powershell
   cd c:\xampp\htdocs\sevro\Sevro
   dotnet ef database update
   ```
   *(Or in Visual Studio Package Manager Console: `Update-Database`.)*
4. Launch the application:
   ```powershell
   dotnet run
   ```
   Open `http://localhost:5180` in your browser.
   - On first startup, `DbInitializer` seeds only the 3 system roles (`Admin`, `Customer`, `Provider`) and the single initial **Admin** login. All categories, providers, services, customers, addresses, bookings, and reviews are created dynamically through the application.

---

## Initial Admin Account

| Role | Email | Password |
|---|---|---|
| **Admin** | `admin@sevro.local` | `Admin@123` |

*(No fake/demo customers, providers, categories, or bookings are seeded.)*

---

## Key Implemented Modules
1. **Authentication & User Management**: Split-card Login, tabbed Customer/Provider Registration, role-based route protection (`[Authorize(Roles = "...")]`).
2. **Service Category Management**: Full Admin CRUD with delete protection when services are linked.
3. **Provider Management & Verification**: Business profile setup, Admin Verify/Reject workflow, and one-click Available/Unavailable live switch.
4. **Service Management & Public Catalog (`/Services`)**: Sticky filter rail (Available Now switch, category radio list, price range checkboxes, city dropdown), search bar, Sort By dropdown, removable filter pills, and 3-column service cards.
5. **Booking Management**: Two-column checkout (saved address ownership check + date-only selection), strict 6-state lifecycle (`Pending -> Confirmed -> InProgress -> Completed` | `Rejected` / `Cancelled`), linear progress stepper, and customer cancellation in `Pending` status.
6. **Review & Rating Management**: Interactive 5-star review modal unlocked strictly after a booking is `Completed` (unique one-review-per-booking database constraint).
