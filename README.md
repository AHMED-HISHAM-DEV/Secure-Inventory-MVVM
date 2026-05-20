# 🛡️ CyberStock - Enterprise Inventory & Warehouse Management System

CyberStock is an advanced, high-performance Inventory and Warehouse Management System designed with a futuristic neon-pixel interface. Built using modern desktop engineering patterns and integrated with robust cybersecurity controls, it provides real-time tracking, dual-method auditing, and highly secure data integrity.


## 🚀 Architectural Design

The application strictly implements the **MVVM (Model-View-ViewModel)** design pattern utilizing the modern Microsoft Community Toolkit. This guarantees complete **Separation of Concerns (SoC)** between the UI presentation layer and the underlying backend logic.

┌────────────────────────────────────────────────────────┐
│                      VIEW (XAML)                       │
└──────────────────────────▲─────────────────────────────┘
│ Data Binding & Commands
┌──────────────────────────▼─────────────────────────────┘
│                   VIEWMODEL (C# Class)                 │
└──────────────────────────▲─────────────────────────────┘
│ Async Method Calls
┌──────────────────────────▼─────────────────────────────┘
│               SERVICES / REPOSITORIES                  │
└──────────────────────────▲─────────────────────────────┘
│ ORM Queries (LINQ)
┌──────────────────────────▼─────────────────────────────┘
│                 DATA LAYER (EF Core)                   │
└──────────────────────────▲─────────────────────────────┘
│ Parameterized SQL
┌──────────────────────────▼─────────────────────────────┘
│                   SQLITE DATABASE                      │
└────────────────────────────────────────────────────────┘

* **View:** Implements custom design systems, neon styles, and UI value converters (`NullToVisibilityConverter`) to deliver an adaptive user interface.
* **ViewModel:** The command center of the application. It manages background asynchronous operations, thread synchronization via the UI Dispatcher, and automatic property notifications.
* **Services / Repositories:** Abstracted database access layer (`ProductRepository`, `TransactionRepository`) ensuring data reusability and keeping ViewModels completely clean.
* **Data Layer:** Managed by Entity Framework Core to orchestrate data mapping, identity tracking, and automatic schema deployment.

---

## 🛠️ Production Tech Stack

* **Presentation Framework:** WPF (.NET 8.0)
* **MVVM Framework:** CommunityToolkit.Mvvm (Source Generators)
* **Object-Relational Mapper:** Microsoft.EntityFrameworkCore 
* **Database Engine:** SQLite (Embedded, self-contained architecture)
* **Cryptographic Engine:** BCrypt.Net-Next

---

## 🛡️ Core Features & Cyber Security Hardening

### 1. Cryptographic Identity Protection
User authentication bypasses standard plaintext pitfalls by forcing all storage to undergo **BCrypt adaptive hashing**. Administrative default seeds (`root`) are securely hashed during database creation, completely mitigating credential dumping attacks.

### 2. Full SQL Injection Mitigation
By executing all data operations through EF Core's object mapping, the system translates abstraction layers into native, **fully parameterized SQL commands**. Raw concatenation is avoided entirely, closing the door on input-based injection vectors.

### 3. Non-Blocking Live Terminal Logging
The system includes an administrative real-time auditing terminal fed by an `ObservableCollection`. All performance-intensive statistical calculations (`CalculateStats`) run asynchronously in background worker threads, avoiding any possibility of UI thread freezing (Race Condition mitigation).

### 4. Dual-Method Scoped Search Architecture
The core search functionality operates under a dual-layer filtering matrix:
* **Global Text Filter:** Instantly parsing Product Names and Barcode IDs using LINQ expressions.
* **Network Scope Filter:** Isolating the dataset to specific physical warehouse boundaries.
Both filters can execute concurrently to narrow query results precisely without performance degradation.

### 5. Single-File Commercial Publishing
Configured for enterprise-grade deployment, the solution compiles into a **Self-Contained, Single-File Executive** (`.exe`). All required framework runtimes and third-party native DLLs are packed directly inside the binary, eliminating environment dependencies on the client host machines.
