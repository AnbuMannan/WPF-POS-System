# WPF POS System (Desktop Edition)

A **Point of Sale (POS) & Inventory Management System** built with **WPF (.NET 8)** and **MVVM**, backed by **ASP.NET Core Web API** and **MySQL**. Designed as a modular, scalable platform for retail and multi-branch use. This document describes the project from A to Z so that anyone can understand the overall system.

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

The system is a **multi-project solution** with clear separation of UI, business APIs, and supporting services. The WPF desktop client talks to the main business API for all master data, billing, reports, and inventory; optional services handle authentication and licensing.

| Project | Description |
|--------|-------------|
| **POS.UI** | WPF desktop client (MVVM). All master screens, billing, payment, hold/draft bills, EOD report, audit log, settings; communicates with APIs via `HttpClient`. |
| **POS.API** | Main business API: Products, Categories, Brands, Tax Profiles, Customers, UoM, Billing (sales, payments, hold/draft), EOD reports, Audit log, Invoices, GST reports, Inventory. Uses clean architecture and MySQL. |
| **POS.AuthService** | Authentication & user service (login, tokens). |
| **POS.LicenseServer** | Licensing and activation service. |
| **POS.Shared** | Shared DTOs used by API and UI (e.g. `ProductDto`, `CustomerDto`, `CreateSaleDto`, `EODReportDto`, `ReceiptDto`). |
| **POS.Hardware** | Hardware abstractions (e.g. ESC/POS printer interface, mock implementation). |
| **POS.UI.Tests** | Unit tests for the UI layer. |

**POS.Core** is a solution folder containing the main API and its layers:

- **POS.API** – ASP.NET Core Web API (controllers, middleware, EF migrations, hosted services).
- **POS.Application** – Application services, repository interfaces, DTOs, validation, tax calculation.
- **POS.Domain** – Domain entities (`Product`, `Category`, `Brand`, `TaxProfile`, `Customer`, `Sale`, `SaleItem`, `Payment`, `HeldBill`, `DraftBill`, `Uom`, etc.) and `BaseEntity` (Id, IsActive, CreatedAt, UpdatedAt); enums (e.g. `PaymentMethod`, `SaleStatus`).
- **POS.Infrastructure** – Data access: `PosDbContext` (EF Core), MySQL connection, repositories, and domain services (Billing, Payment, HeldBill, DraftBill, Receipt, Return, EOD Report, Audit Log).

The architecture supports:

- End-to-end billing: cart, tax (CGST/SGST/IGST), round-off, payment, receipt, hold/draft.
- EOD (End-of-Day) report and day-close; audit log for key actions.
- Multi-branch and centralized data readiness; consistent master data and validation.

---

## 🛠️ Technology Stack

| Layer | Technologies |
|-------|--------------|
| **Desktop (WPF)** | .NET 8, WPF, MVVM, XAML, DataBinding, ICommand, Converters, ResourceDictionary styles (Forms, DataGrid, Buttons, Converters), ThemeManager |
| **API** | ASP.NET Core 8, REST, Swagger/OpenAPI, AutoMapper, JSON (ReferenceHandler.IgnoreCycles) |
| **Data** | MySQL, Entity Framework Core 8, Pomelo.EntityFrameworkCore.MySql, Dapper (where used) |
| **Desktop → API** | `HttpClient`, `IHttpClientFactory`, Polly (Retry, Circuit Breaker), per-module ApiService classes (e.g. BillingApiService, EODReportApiService) |
| **Logging** | Serilog (file + debug) in WPF client |
| **Auth / License** | JWT-style auth (AuthService), custom license activation (LicenseServer) |

---

## 🏗️ Solution Structure

```
POS (repo root)
├── POS_System.sln              ← Open this solution
├── POS.UI/                     (WPF client)
│   ├── Modules/
│   │   ├── Admin/              (Products, Categories, Brands, TaxProfiles, Customers, UoM; Barcode Label under Products)
│   │   ├── Authentication/    (Login)
│   │   ├── Billing/            (BillingScreen, PaymentDialog, HoldBill, DraftBill, HeldBills, DraftBills,
│   │   │                       ProductSearch, QuickSale, ReceiptPreview, ReturnDialog, Shortcuts, CustomerDisplay)
│   │   ├── Inventory/         (StockView)
│   │   ├── Reports/           (GSTReports, EODReport, AuditLog)
│   │   └── Settings/         (SettingsView, PrintSettingsDialog)
│   ├── Core/                   (MVVM base, ViewModelBase, RelayCommand; ApiServices; Converters; Navigation, ViewResolver; AppState; Helpers)
│   ├── Infrastructure/        (Bootstrapper, DI, HttpClient + Polly)
│   ├── Styles/, Themes/       (Forms, DataGrid, Buttons, Converters, BrandTheme)
│   ├── Components/            (DialogService, MessageDialog)
│   ├── Models/                (e.g. CartItem)
│   └── appsettings.json       (Api base URL, Auth/License URLs, Store state, timeouts, resilience)
├── POS.Core/                   (solution folder)
│   ├── POS.API/                (Web API, Controllers, Migrations, Services e.g. HeldBillCleanupService)
│   ├── POS.Application/       (Interfaces + Services, Repository interfaces)
│   ├── POS.Domain/             (Entities, Enums)
│   └── POS.Infrastructure/     (DbContext, Repositories, Services: Billing, Payment, HeldBill, DraftBill, Receipt, Return, EODReport, AuditLog)
├── POS.Shared/                 (Shared DTOs: ProductDto, CustomerDto, CreateSaleDto, CartDataDto, EODReportDto, ReceiptDto, etc.)
├── POS.AuthService/
├── POS.LicenseServer/
├── POS.Hardware/
└── POS.UI.Tests/
```

---

## 🧩 Architecture

### Backend (POS.API + POS.Core)

- **Layered / Clean style**: API → Application → Domain; Infrastructure implements repositories and domain services.
- **Domain**: Entities inherit `BaseEntity` (e.g. Guid Id, IsActive, CreatedAt, UpdatedAt). Soft delete / filtering by `IsActive` where applicable. Enums for PaymentMethod, PaymentStatus, SaleStatus, SaleType.
- **Application**: Services orchestrate use cases and validation; they use repository interfaces and throw validation exceptions for business rule failures.
- **Infrastructure**:
  - `PosDbContext` (EF Core) + migrations in **POS.API**.
  - Repositories: Product, Category, Brand, TaxProfile, Customer, Invoice, Uom, Inventory, GstReport.
  - Services: BillingService, PaymentService, HeldBillService, DraftBillService, ReceiptService, ReturnService, EODReportService, AuditLogService; hosted `HeldBillCleanupService` for held-bill expiry.
- **API**: Controllers call application/infrastructure services; validation errors mapped to appropriate HTTP responses. Swagger for discovery.

### Frontend (POS.UI)

- **MVVM**: Views (XAML), ViewModels (commands, properties, INotifyPropertyChanged), Models/DTOs. `ViewModelBase`, `RelayCommand`, shared styles (Forms, DataGrid, Buttons).
- **ApiService per area**: ProductApiService, CategoryApiService, BrandApiService, TaxProfileApiService, CustomerApiService, UomApiService, BillingApiService, EODReportApiService, ReturnApiService, AuditLogApiService, etc. All use `BaseApiService` and base URL from config.
- **Navigation**: MainWindow drives sidebar menu (e.g. Sales → Billing, EOD Report); ViewResolver resolves view names to types (e.g. BillingView, EODReportView). Optional menu structure in `menu.json`.
- **DI**: Bootstrapper configures `IHttpClientFactory`, Polly policies, and registers all ApiServices, DialogService, PrintService, etc.

### Data Flow

1. User action in WPF → ViewModel command → ApiService (e.g. `BillingApiService`) → HTTP call to POS.API.
2. API Controller → Application/Infrastructure Service → Repository / DbContext → MySQL.
3. Response (DTO) → ApiService → ViewModel → View binding.

---

## ✅ Implemented Modules

### Core foundation

- MVVM base (`ViewModelBase`, `RelayCommand`), ApiService pattern, WPF ↔ API connection.
- Theming (BrandTheme.xaml), validation display (e.g. `FirstValidationErrorConverter`), form styles (Forms.xaml), DataGrid styles, multiple value converters (BoolToVisibility, DecimalToCurrency, NullToVisibility, etc.).
- Resilience: Polly retry and circuit breaker for API calls; configurable timeouts.

### Master data (Admin)

| Module | Backend | WPF | Features |
|--------|---------|-----|----------|
| **Products** | CRUD API, Category/Brand/TaxProfile/UoM lookups | List + Form | Add/Edit/Disable, category/brand/tax/UoM, selling price, tax-inclusive, search, show inactive |
| **Categories** | CRUD API, parent-child hierarchy | List + Form | Root + sub-category, Add/Edit/Disable, search, show inactive |
| **Brands** | CRUD API | List + Form | Add/Edit/Disable, search, show inactive |
| **Tax Profiles** | CRUD API | List + Form | Add/Edit/Disable, search, show inactive |
| **Customers** | CRUD API | List + Form | Customer master CRUD |
| **UoM** | CRUD API | List + Form | Units of measure CRUD |
| **Barcode Label** | — | Dialog + View | Barcode label printing/preview from product data |

Common behavior across masters: **Search**, **Clear**, **Show inactive** (API `includeInactive` where supported), validation and error display in forms.

### Authentication

- Login flow (LoginView / LoginViewModel); integration with AuthService for tokens/session.

### Billing (full flow)

| Component | Description |
|-----------|-------------|
| **BillingScreen** | Cart grid, product search (code/name/barcode), quantity +/- , price/discount/tax; **Total** (before round-off) and **Net Payable** (after round-off); customer, discount %, bill #, reserve bill #. |
| **PaymentDialog** | Pay by Cash/Card/UPI/etc.; amount tendered; complete sale → creates Sale, Payments, Receipt. |
| **Hold Bill** | Hold current cart with optional reason; persisted via HeldBill API. |
| **Held Bills** | List and recall held bills. |
| **Draft Bill** | Save current cart as draft with name; persisted via DraftBill API. |
| **Draft Bills** | List and load draft bills. |
| **ProductSearch** | Search product by code, name, barcode; add to cart (with quantity). |
| **QuickSale** | Quick-sale entry (product + qty). |
| **ReceiptPreview** | Print preview of receipt (store info, line items, totals, payment summary). |
| **ReturnDialog** | Sales return flow (linked to sale/refund). |
| **Shortcuts** | Keyboard shortcuts help (F1 Search, F2 Hold, F12 Pay, etc.). |
| **CustomerDisplay** | Secondary display window for customer-facing total/items. |

Backend: **BillingController** (create sale, hold, draft, recall, complete payment), **PaymentService**, **ReceiptService**, **HeldBillService**, **DraftBillService**, **ReturnService**; entities Sale, SaleItem, Payment, HeldBill, DraftBill, BillSequence; EOD uses sale locking (e.g. SaleLocked columns).

### Reports

| Module | Backend | WPF | Features |
|--------|---------|-----|----------|
| **EOD Report** | EODReportsController, EODReportService | EODReportView, EODReportViewModel | Date range, summary (revenue, payment breakdown, tax CGST/SGST/IGST, returns, cash reconciliation), top sales, top products; Generate, Print, Close Day, Export (e.g. CSV). |
| **Audit Log** | AuditLogController, AuditLogService | AuditLogView | View audit trail of key actions (entity, action, user, time). |
| **GST Reports** | GstReportsController | GstReportView | GST report data from API. |

### Inventory

- **StockView** – Stock/inventory view; backend InventoryController and inventory repositories for stock data.

### Settings

- **SettingsView** – Application/settings entry.
- **PrintSettingsDialog** – Print and receipt settings (PrintService, IPrintSettingsService).

### API overview

Main REST controllers (base URL from `Api:BaseUrl`, e.g. `https://localhost:7285`):

| Area | Route | Main actions |
|------|--------|--------------|
| Products | `api/products` | GET (by id, barcode, search), POST, PUT, PATCH (disable) |
| Categories | `api/categories` | GET (includeInactive), POST, PUT, PATCH (disable) |
| Brands | `api/brands` | GET (includeInactive), POST, PUT, PATCH (disable) |
| Tax Profiles | `api/taxprofiles` | GET (includeInactive), POST, PUT, PATCH (disable) |
| Customers | `api/customers` | GET, POST, PUT, PATCH (disable) |
| UoM | `api/uoms` | GET, POST, PUT |
| Billing | `api/billing` | POST create sale, hold, draft, recall, complete payment, etc. |
| EOD Reports | `api/eod-reports` | GET report by date, POST close-day |
| Audit Log | `api/audit-log` | GET (filter by entity, date, etc.) |
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

3. **Run EF migrations** (from repo root):

   ```bash
   dotnet ef database update --project POS.Core/POS.API --startup-project POS.Core/POS.API
   ```

4. **WPF client** – in `POS.UI/appsettings.json` (and `appsettings.Development.json` if used):
   - `ApiSettings:BaseUrl` → POS.API base URL (e.g. `https://localhost:7285/`)
   - `AuthSettings:BaseUrl` / `LicenseBaseUrl` → Auth and License service URLs if you use login and licensing.

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
| POS.API `appsettings.json` | `Receipt:*` | Store name, address, phone, email, GSTIN, footer, thank-you message |
| POS.AuthService `appsettings.json` | `ConnectionStrings:MySql` | Auth database (pos_auth) |
| POS.LicenseServer `appsettings.json` | `ConnectionStrings:MySql` | License database |
| POS.UI `appsettings.json` | `ApiSettings:BaseUrl` | Base URL of POS.API |
| POS.UI `appsettings.json` | `ApiSettings:TimeoutSeconds`, `RetryCount`, `CircuitBreakerFailureThreshold`, `CircuitBreakerTimeoutSeconds` | HTTP timeout and Polly resilience |
| POS.UI `appsettings.json` | `AuthSettings:BaseUrl`, `LicenseBaseUrl` | Auth and license service URLs |
| POS.UI `appsettings.json` | `StoreSettings:StoreState` | Store state (e.g. for SGST/CGST vs IGST) |
| POS.UI `appsettings.json` | `Logging:LogLevel` | Log level (Default, Microsoft) |

---

## 🧭 Roadmap

- **Short term:** Sales return UI wiring end-to-end; inventory stock-in/stock-out flows; GST report wiring to UI.
- **Mid term:** User/role management; more reports; printer integration (POS.Hardware/ESC-POS); email receipt.
- **Long term:** Multi-branch support, sync, advanced analytics, offline capability.

---

## 🤝 Contributing & License

Contributions are welcome. Use feature branches and pull requests with clear messages. This project is currently **proprietary**; add license details as needed.

---

## Summary

This repository is a **WPF POS and inventory platform** with:

- **.NET 8** WPF client (MVVM) and ASP.NET Core API in a clean, layered structure.
- **Full billing**: cart, tax (CGST/SGST/IGST), round-off (Total vs Net Payable), payment, receipt, hold/draft bills, sales return.
- **EOD Report** with day-close and cash reconciliation; **Audit Log** for key actions.
- **Master data**: Products, Categories, Brands, Tax Profiles, Customers, UoM (CRUD, search, show inactive); Barcode Label UI.
- **Settings** and **Print** integration; **Resilience** (Polly) and **Logging** (Serilog) in the client.
- **Separate** Auth and License services; **shared DTOs** for API and UI.

Designed for clarity, maintainability, and future scaling (multi-branch, reporting, inventory).
