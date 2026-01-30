# POS System Development Standards

## 1. Project Architecture (Strict)
- **POS.UI:** WPF (XAML) only. Strictly follow MVVM. No business logic in code-behind.
- **POS.Core:** Business logic, Domain Models, and Entity Framework. Use Repository/Service patterns.
- **POS.AuthService:** Isolated login/licensing logic. Use JWT or encrypted tokens.
- **POS.LicenseServer:** Centralized validation logic. Use high-security hashing (SHA-256/512).

## 2. WPF & UI/UX Standards (2026 Focus)
- **Library:** Use CommunityToolkit.Mvvm for ViewModels and Commands.
- **Async UI:** All DB/API calls must be `async/await`. Never block the UI thread.
- **Animations:** Use subtle `VisualStateManager` transitions for a premium feel.
- **Touch-Friendly:** Ensure buttons have a minimum hit target of 40x40px for retail touchscreens.
- **Responsiveness:** Use `Grid` with star/auto sizing; avoid fixed pixel widths for multi-monitor support.

## 3. Data & Entity Framework (MySQL)
- **Provider:** Use `Pomelo.EntityFrameworkCore.MySql`.
- **Performance:** - Always use `.AsNoTracking()` for read-only checkout/inventory displays.
    - Use `Projection` (`.Select()`) instead of loading entire entities.
- **Reliability:** Implement a "Local First" strategy. If the database connection drops, the UI should gracefully alert the user but keep the current transaction in memory.

## 4. Security & Licensing
- **Sensitive Data:** Never store passwords or license keys in plain text. Use `BCrypt.Net` for hashing.
- **License Validation:** The POS must work offline for 24 hours after the "first-time" activation from POS.LicenseServer. 
- **API Security:** All communication between POS.UI and LicenseServer must be over HTTPS with encrypted payloads.

## 5. Coding Style
- **Naming:** Follow Microsoft C# Coding Conventions. Use meaningful names (e.g., `ProcessTransactionAsync` not `DoWork`).
- **DI:** Use Microsoft.Extensions.DependencyInjection for all projects.
- **Error Handling:** Use a global exception handler in WPF (`DispatcherUnhandledException`) to log errors to a local file without crashing the app.

## 6. Hardware Abstraction Layer (HAL)
- **Modularity:** Use interfaces (e.g., `IPrinter`, `IScanner`) in `POS.Hardware`. 
- **Implementation:** Implement specific drivers (e.g., `EpsonReceiptPrinter`, `HoneywellScanner`) using these interfaces.
- **Safety:** Wrap all hardware calls in `try-catch` blocks. If a printer is offline, the system must not crash; it should queue the print or notify the user.
- **Communication:** Use `SerialPort` or `TCP/IP` for hardware communication, ensuring all streams are properly disposed of.

## 7. Theming & UX (BrandTheme.xaml)
- **Consistency:** All UI elements must use DynamicResource references from `BrandTheme.xaml` (e.g., `{DynamicResource PrimaryBrandColor}`).
- **Future-Proofing:** Do not hardcode colors or font sizes in local XAML files. 
- **Dark Mode:** Support light/dark switching by defining theme dictionaries that can be swapped at runtime.
- **Scaling:** Use ViewBox or DPI-aware layouts to ensure the POS looks identical on a 15" touch terminal and a 24" desktop monitor.

## 8. Enterprise Error Handling & Logging
- **Global Catch:** Implement `App.DispatcherUnhandledException` in `POS.UI` to show a user-friendly "Oops" dialog while logging the technical stack trace in the background.
- **Logging:** Use Serilog or NLog to write logs to `C:\ProgramData\YourPOS\Logs`. 
- **API Errors:** Standardize API responses using a `Result<T>` pattern (Success/Failure with ErrorMessage) to avoid throwing raw exceptions across the network.