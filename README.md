# WPF POS System (Desktop Edition)

A professional **Desktop Point of Sale (POS) & Inventory Management System** built with **WPF (.NET 8)** using **MVVM architecture**, backed by an **ASP.NET Core Web API** and **MySQL** database.
This project is designed as a **commercial‑grade retail POS platform** with modular, scalable architecture for long‑term real‑world use.

---

## 📌 Project Overview

This system is a multi‑project solution consisting of:

* **POS.UI** – WPF Desktop Client (MVVM)
* **POS.AuthService** – Authentication / User service (Web API)
* **POS.LicenseServer** – Licensing & activation service
* **POS.Core** – Shared core models, DTOs, helpers, and business logic

The architecture separates UI, business logic, and backend services so the system can later scale to:

* Multi‑branch deployments
* Centralized data sync
* Advanced reporting & analytics

---

## 🛠️ Technology Stack

### Frontend (Desktop)

* WPF (.NET 8)
* MVVM Architecture
* XAML DataBinding, Commands, Converters, Styles

### Backend

* ASP.NET Core Web API (.NET 8)

### Database

* MySQL

### Communication

* WPF → Web API via `HttpClient`
* Separate ApiService layer per module

---

## 🏗️ Solution Structure

```
POS-System
 ├── POS.UI            (WPF Desktop Client)
 ├── POS.AuthService   (Authentication Web API)
 ├── POS.LicenseServer (License & Activation Server)
 ├── POS.Core          (Shared Core Library)
 └── POS.sln           (Visual Studio Solution)
```

---

## 🧩 Architecture

* Clean **MVVM layered structure** in WPF:

  * Views
  * ViewModels
  * Models
  * Services (ApiServices)

* Backend exposes REST APIs for:

  * Authentication (planned / partial)
  * Product Management
  * Category Management
  * Stock / Inventory (planned)

The WPF client handles only UI and presentation logic.
All data operations are performed through the Web API.

---

## ✅ Implemented Modules

### 🔹 Core Foundation

* MVVM base setup (`BaseViewModel`, `RelayCommand`)
* ApiService pattern implemented
* WPF successfully connected with Web API
* MySQL integrated with backend

---

### 🔹 Product Module (Stable Reference Module)

Backend:

* Product CRUD APIs
* MySQL product tables

WPF:

* Product List View
* Product Form View
* Features:

  * Load product list from API
  * Add product
  * Edit product
  * Disable product

This module acts as the **reference template** for other modules.

---

### 🔹 Category Module (Almost Complete)

Backend:

* APIs for:

  * Get all categories
  * Add Root Category
  * Add Sub‑Category (Parent–Child hierarchy)
  * Edit Category
  * Enable / Disable Category

WPF:

* Category List View
* Category Form View
* Features:

  * Load categories
  * Search
  * Add Root / Sub Category
  * Edit Category
  * Disable Category

Technical fixes completed:

* ResourceDictionary wiring
* Missing converters and styles
* Command and binding flow corrections

Result: Category hierarchy and CRUD flow working correctly.

---

## 🚧 Modules Planned / In Progress

* 🔐 Authentication & User Roles

* 📦 Stock / Inventory Management

  * Stock In / Stock Out
  * Monthly stock records per product
  * Low‑stock alerts

* 🧾 POS Billing

  * Cart system
  * Barcode / QR scanning
  * Invoice & printing

* 📊 Reports & Analytics

  * Daily / monthly sales
  * Stock reports

---

## 🚀 Getting Started

### Prerequisites

* Visual Studio 2022 or later
* .NET SDK 8.0+
* MySQL Server
* Git

---

### Setup Steps

1. Clone the repository:

```bash
git clone ttps://github.com/AnbuMannan/WPF-POS-System.git
```

2. Open the solution:

```bash
POS.sln
```

3. Configure database connection strings in:

* `POS.AuthService` → `appsettings.json`
* `POS.LicenseServer` → `appsettings.json`

4. Run database migrations / create tables (if applicable).

5. Start backend services:

* POS.AuthService
* POS.LicenseServer

6. Run the WPF client:

* POS.UI

---

## 🧭 Roadmap

Short term:

* Final polish of Product & Category modules
* Implement Stock / Inventory module

Mid term:

* Authentication & role management
* POS Billing module

Long term:

* Reporting dashboard
* Multi‑branch support
* Cloud sync

---

## 🤝 Contributing

This project follows a modular architecture.
Please create feature branches and submit pull requests with clear commit messages.

---

## 📄 License

This project is currently private / proprietary.
License details can be added later.

---

## ✨ Summary

This project is a **commercial‑grade WPF POS & Inventory System** built with:

* .NET 8
* MVVM architecture
* ASP.NET Core Web API
* MySQL backend

Designed for scalability, stability, and real‑wor
