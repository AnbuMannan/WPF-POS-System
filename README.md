# WPF POS System (Desktop Edition)

A **Point of Sale (POS) & Inventory Management System** built with **WPF (.NET 8)** and **MVVM**, backed by **ASP.NET Core Web API** and **MySQL**. Designed as a modular, scalable platform for retail and multi-branch use.

---

## Table of Contents

- [Project Overview](#-project-overview)
- [Technology Stack](#-technology-stack)
- [Solution Structure](#-solution-structure)
- [Architecture](#-architecture)
- [Implemented Modules](#-implemented-modules)
- [Getting Started](#-getting-started)
- [Configuration](#-configuration)
- [Roadmap](#-roadmap)
- [Contributing & License](#-contributing--license)

---

## 📌 Project Overview

The system is a **multi-project solution** with clear separation of UI, business APIs, and supporting services:

| Project | Description |
|--------|-------------|
| **POS.UI** | WPF desktop client (MVVM). All master screens, billing, reports; communicates with APIs via `HttpClient`. |
| **POS.API** | Main business API (Products, Categories, Brands, Tax Profiles, Customers, Invoices, GST reports, Inventory, UoM). Uses clean architecture and MySQL. |
| **POS.AuthService** | Authentication & user service (login, tokens). |
| **POS.LicenseServer** | Licensing and activation service. |
| **POS.Shared** | Shared DTOs used by API and UI (e.g. `ProductDto`, `CategoryDto`, `BrandDto`, `TaxProfileDto`, `CustomerDto`). |
| **POS.Hardware** | Hardware abstractions (e.g. ESC/POS printer interface, mock implementation). |
| **POS.UI.Tests** | Unit tests for the UI layer. |

**POS.Core** is a solution folder containing the main API and its layers:

- **POS.API** – ASP.NET Core Web API (controllers, middleware, EF migrations).
- **POS.Application** – Application services, repository interfaces, DTOs, validation, tax calculation.
- **POS.Domain** – Domain entities (`Product`, `Category`, `Brand`, `TaxProfile`, `Customer`, `Invoice`, `Uom`, etc.) and `BaseEntity` (Id, IsActive, CreatedAt, UpdatedAt).
- **POS.Infrastructure** – Data access: `PosDbContext` (EF Core), MySQL connection, and repositories (Dapper/EF as applicable).

The architecture supports:

- Multi-branch and centralized data
- Consistent master data and validation
- Future reporting and inventory workflows

---

## 🛠️ Technology Stack

| Layer | Technologies |
|-------|--------------|
| **Desktop (WPF)** | .NET 8, WPF, MVVM, XAML, DataBinding, Commands, Converters, ResourceDictionary styles |
| **API** | ASP.NET Core 8, REST, Swagger/OpenAPI, AutoMapper |
| **Data** | MySQL, Entity Framework Core 8, Dapper, Pomelo.EntityFrameworkCore.MySql |
| **Desktop → API** | `HttpClient`, `IHttpClientFactory`, Polly (retry/circuit breaker), per-module ApiService classes |
| **Logging** | Serilog (file + debug) in WPF client |
| **Auth / License** | JWT-style auth (AuthService), custom license activation (LicenseServer) |

---

## 🏗️ Solution Structure

```
POS (repo root)
├── POS_System.sln              ← Open this solution
├── POS.UI/                     (WPF client)
│   ├── Modules/
│   │   ├── Admin/              (Products, Categories, Brands, TaxProfiles, Customers)
│   │   ├── Authentication/
│   │   ├── Billing/
│   │   ├── Inventory/
│   │   └── Reports/
│   ├── Core/                   (MVVM base, ApiServices, Converters, Navigation)
│   ├── Infrastructure/        (Bootstrapper, DI)
│   ├── Styles/, Themes/
│   └── menu.json               (sidebar menu definition)
├── POS.Core/                   (solution folder)
│   ├── POS.API/                (Web API, Controllers, Migrations)
│   ├── POS.Application/       (Services, Repository interfaces, DTOs)
│   ├── POS.Domain/             (Entities)
│   └── POS.Infrastructure/     (DbContext, Repositories)
├── POS.Shared/                 (Shared DTOs)
├── POS.AuthService/
├── POS.LicenseServer/
├── POS.Hardware/
└── POS.UI.Tests/
```

---

## 🧩 Architecture

### Backend (POS.API + POS.Core)

- **Clean / layered style**: API → Application → Domain; Infrastructure implements repositories and data access.
- **Domain**: Entities inherit `BaseEntity` (Guid Id, IsActive, CreatedAt, UpdatedAt, RowVersion). Soft delete / filtering by `IsActive` where applicable.
- **Application**: Services perform orchestration and validation; they use repository interfaces and throw `ValidationException` for business rule failures.
- **Infrastructure**: 
  - `PosDbContext` (EF Core) + migrations in **POS.API**.
  - Repositories for Product, Category, Brand, TaxProfile, Customer, Invoice, Uom, Inventory, GstReport.
  - MySQL via EF Core and/or Dapper as needed.
- **API**: Controllers call application services; validation errors mapped to appropriate HTTP responses. Swagger for discovery.

### Frontend (POS.UI)

- **MVVM**: Views (XAML), ViewModels (commands, properties), Models/DTOs. `ViewModelBase`, `RelayCommand`, and shared styles (Forms, DataGrid, Buttons, etc.).
- **ApiService per area**: e.g. `ProductApiService`, `CategoryApiService`, `BrandApiService`, `TaxProfileApiService`, `CustomerApiService`. All use a shared `BaseApiService` and base URL from config.
- **Navigation**: Menu driven by `menu.json`; ViewResolver and MenuService resolve headers to views (e.g. ProductView, TaxProfileView).
- **DI**: Bootstrapper configures `IHttpClientFactory`, Polly policies, and registers all ApiServices and other dependencies.

### Data Flow

1. User action in WPF → ViewModel command → ApiService (e.g. `ProductApiService`) → HTTP call to POS.API.
2. API Controller → Application Service → Repository → MySQL.
3. Response (DTO) → ApiService → ViewModel → View binding.

---

## ✅ Implemented Modules

### Core foundation

- MVVM base (`ViewModelBase`, `RelayCommand`), ApiService pattern, WPF ↔ API connection.
- Theming (BrandTheme.xaml), validation display (e.g. `FirstValidationErrorConverter`), form styles (Forms.xaml), DataGrid styles.

### Master data (Admin)

| Module | Backend | WPF | Features |
|--------|---------|-----|----------|
| **Products** | CRUD API, Category/Brand/TaxProfile/UoM lookups | List + Form | Add/Edit/Disable, category/brand/tax dropdowns, selling price, tax-inclusive flag, search, show inactive |
| **Categories** | CRUD API, parent-child hierarchy | List + Form | Root + sub-category, Add/Edit/Disable, search, show inactive |
| **Brands** | CRUD API | List + Form | Add/Edit/Disable, search, clear, show inactive |
| **Tax Profiles** | CRUD API | List + Form | Add/Edit/Disable, search, clear, show inactive |
| **Customers** | CRUD API | List + Form | Customer master CRUD |

Common behavior across masters:

- **Search** (debounced where applicable) and **Clear**.
- **Show inactive** checkbox: API supports `includeInactive`; list and filters reflect active/inactive state.
- Validation and error display in forms (e.g. first validation error message).

### Authentication

- Login flow (LoginView / LoginViewModel); integration with AuthService for tokens/session.

### Billing, Inventory, Reports

- Billing screen and inventory/report placeholders exist; backend has controllers and services for Invoices, Inventory, and GST reports for future completion.

### API overview

Main REST controllers (base URL from `Api:BaseUrl`, e.g. `https://localhost:7285`):

| Area | Base route | Main actions |
|------|------------|--------------|
| Products | `api/products` | GET (by id, barcode, search), POST, PUT, PATCH (disable) |
| Categories | `api/categories` | GET (includeInactive), POST, PUT, PATCH (disable) |
| Brands | `api/brands` | GET (includeInactive), POST, PUT, PATCH (disable) |
| Tax Profiles | `api/taxprofiles` | GET (includeInactive), POST, PUT, PATCH (disable) |
| Customers | `api/customers` | GET, POST, PUT, PATCH (disable) |
| UoM | `api/uoms` | GET, POST, PUT |
| Invoices | `api/invoices` | POST (create) |
| GST Reports | `api/gstreports` | Report endpoints |
| Inventory | `api/inventory` | Stock-related endpoints |

Swagger UI is available in Development at `/swagger`.

---

## 🚀 Getting Started

### Prerequisites

- **Visual Studio 2022** (or later) or **VS Code** with C# / .NET workload
- **.NET 8 SDK**
- **MySQL Server**
- **Git**

### Clone and open

```bash
git clone https://github.com/AnbuMannan/WPF-POS-System.git
cd WPF-POS-System
```

Open **`POS_System.sln`** in Visual Studio (or your IDE).

### Database and configuration

1. **Create MySQL databases** (or adjust connection strings to match your DBs):
   - Main store: e.g. `pos_store` (used by POS.API)
   - Auth: e.g. `pos_auth` (POS.AuthService)
   - License: e.g. `pos_license_server` (POS.LicenseServer)

2. **Connection strings** – update in:
   - `POS.Core/POS.API/appsettings.json` → `ConnectionStrings:MySql` (pos_store)
   - `POS.AuthService/appsettings.json` → `ConnectionStrings:MySql` (pos_auth)
   - `POS.LicenseServer/appsettings.json` → `ConnectionStrings:MySql` (pos_license_server)

3. **Run EF migrations** (from repo root, with .NET 8):

   ```bash
   dotnet ef database update --project POS.Core/POS.API --startup-project POS.Core/POS.API
   ```

4. **WPF client API base URL** – in `POS.UI/appsettings.json` (and `appsettings.Development.json` if used), set `Api:BaseUrl` to your POS.API base URL (e.g. `https://localhost:7285/`). Ensure Auth and License URLs match where you host AuthService and LicenseServer.

### Run order

1. Start **POS.API** (main business API).
2. Start **POS.AuthService** and **POS.LicenseServer** if you use login and licensing.
3. Run **POS.UI** (WPF client).

Set POS.API and POS.UI as startup projects or run multiple projects as needed.

---

## ⚙️ Configuration

| Location | Key | Purpose |
|----------|-----|---------|
| POS.API `appsettings.json` | `ConnectionStrings:MySql` | Main store database (pos_store) |
| POS.AuthService `appsettings.json` | `ConnectionStrings:MySql` | Auth database (pos_auth) |
| POS.LicenseServer `appsettings.json` | `ConnectionStrings:MySql` | License database |
| POS.UI `appsettings.json` | `Api:BaseUrl` | Base URL of POS.API |
| POS.UI `appsettings.json` | `Auth:BaseUrl`, `License:LicenseBaseUrl` | Auth and license service URLs |

---

## 🧭 Roadmap

- **Short term:** Finish Billing flow, Inventory (stock in/out), GST report wiring to UI.
- **Mid term:** User/role management, more reports, printer integration (POS.Hardware).
- **Long term:** Multi-branch support, sync, advanced analytics.

---

## 🤝 Contributing & License

Contributions are welcome. Use feature branches and pull requests with clear messages. This project is currently **proprietary**; add license details as needed.

---

## Summary

This repository is a **WPF POS and inventory platform** with:

- **.NET 8** WPF client (MVVM) and ASP.NET Core API
- **Clean architecture** in the API (Domain, Application, Infrastructure)
- **MySQL** via EF Core (and Dapper where used)
- **Master data** modules: Products, Categories, Brands, Tax Profiles, Customers (CRUD, search, show inactive)
- **Separate** Auth and License services and shared DTOs for API and UI

Designed for clarity, maintainability, and future scaling (multi-branch, reporting, inventory).
