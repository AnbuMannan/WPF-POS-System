# Purchase Return Module - Complete Implementation Guide

## Overview
This document provides a comprehensive guide for the **Purchase Return** module implementation in the market-standard POS system. The module handles returning goods to suppliers with complete stock management and ledger tracking.

---

## ✅ Completed Backend Components

### 1. Domain Entities (`POS.Core/POS.Domain/Entities/`)

#### **PurchaseReturn.cs**
- Main entity for tracking purchase returns
- Fields: ReturnNo, ReturnDate, SupplierId, PurchaseEntryId (nullable), TotalAmount, TaxAmount, Reason, Notes, Status, IsProcessed
- Inherits from `BaseEntity`
- Navigation properties: Supplier, PurchaseEntry, Items collection

#### **PurchaseReturnItem.cs**
- Line items for each returned product
- Fields: ProductId, BatchNo, ExpiryDate, Quantity, UnitPrice, TaxAmount, TotalAmount, Reason
- Links to PurchaseReturnId and optionally PurchaseEntryItemId
- Inherits from `BaseEntity`

### 2. Application Layer (`POS.Core/POS.Application/`)

#### **Interfaces/Repositories/IPurchaseReturnRepository.cs**
- Repository contract with methods:
  - GetAllAsync, GetByIdAsync, GetBySupplierAsync, GetByPurchaseEntryIdAsync
  - GetUnprocessedAsync, AddAsync, UpdateAsync, DisableAsync
  - CheckReturnNoExistsAsync
  - **ProcessReturnWithInventoryUpdateAsync** (critical for stock management)

#### **Interfaces/Services/IPurchaseReturnService.cs**
- Service contract with similar methods as repository
- Additional: ProcessReturnAsync (orchestrates the full return process)

#### **Services/PurchaseReturnService.cs**
- Complete business logic implementation
- Validation: supplier exists, products exist, quantities valid, return number unique
- Calculation: automatic total and tax calculation
- Error handling: ValidationException, NotFoundException
- Manual mapping to DTOs

### 3. Infrastructure Layer (`POS.Core/POS.Infrastructure/`)

#### **Repositories/PurchaseReturnRepository.cs**
- EF Core implementation with:
  - Full Include() chains for eager loading (Supplier, PurchaseEntry, Items, Products)
  - Query filters for soft deletes
  - AsNoTracking() for read operations
  - **ProcessReturnWithInventoryUpdateAsync**: CRITICAL transaction logic
    - Reduces stock in `Batches` table (CurrentQuantity decreased, ReturnedQuantity increased)
    - Creates `StockLedgerEntry` records (EntryType: "OUT", ReferenceType: "PURCHASE_RETURN")
    - Updates PurchaseReturn status to "Processed"
    - Uses database transactions for atomicity

### 4. API Layer (`POS.Core/POS.API/`)

#### **Controllers/PurchaseReturnsController.cs**
- RESTful endpoints:
  - GET `/api/purchase-returns` - Get all returns
  - GET `/api/purchase-returns/{id}` - Get single return
  - GET `/api/purchase-returns/supplier/{supplierId}` - Get by supplier
  - GET `/api/purchase-returns/purchase-entry/{purchaseEntryId}` - Get by GRN
  - GET `/api/purchase-returns/unprocessed` - Get unprocessed returns
  - POST `/api/purchase-returns` - Create new return
  - PUT `/api/purchase-returns/{id}` - Update return
  - **POST `/api/purchase-returns/{id}/process`** - Process return (CRITICAL)
  - DELETE `/api/purchase-returns/{id}` - Disable return
  - GET `/api/purchase-returns/check-return-no/{returnNo}` - Check uniqueness

### 5. DTOs (`POS.Shared/Models/`)

#### **PurchaseReturnDto.cs**
- Complete DTO with all fields including computed SupplierName, PurchaseEntryInvoiceNo
- Items collection of PurchaseReturnItemDto

#### **PurchaseReturnItemDto.cs**
- Item-level DTO with ProductName, ProductCode

#### **CreatePurchaseReturnDto.cs**
- Input DTO for create/update operations
- Items collection of CreatePurchaseReturnItemDto

### 6. Database (`POS.Core/POS.Infrastructure/Data/`)

#### **PosDbContext.cs Updates**
- Added DbSets:
  - `DbSet<PurchaseReturn> PurchaseReturns`
  - `DbSet<PurchaseReturnItem> PurchaseReturnItems`
  - `DbSet<StockLedgerEntry> StockLedgerEntries` (new)
- Entity configurations in OnModelCreating:
  - PurchaseReturn: table mapping, indexes, foreign keys, query filters
  - PurchaseReturnItem: table mapping, indexes, relationships
  - StockLedgerEntry: table mapping for audit trail

#### **create_purchase_returns_tables.sql**
- Complete MySQL schema:
  - `PurchaseReturns` table with indexes and foreign keys
  - `PurchaseReturnItems` table with cascade delete
  - `StockLedgerEntries` table for audit trail
- Ready to execute in MySQL database

### 7. Dependency Injection (`POS.Core/POS.API/Program.cs`)
- Added registrations:
  ```csharp
  builder.Services.AddScoped<IPurchaseReturnRepository, PurchaseReturnRepository>();
  builder.Services.AddScoped<IPurchaseReturnService, PurchaseReturnService>();
  ```

---

## ✅ Completed Frontend Components

### 1. API Service (`POS.UI/Core/Services/`)

#### **PurchaseReturnApiService.cs**
- HttpClient-based API communication
- Methods matching backend endpoints
- Exception handling with ApiException
- JSON serialization with case-insensitive options
- All CRUD operations + Process operation

### 2. Dependency Injection (`POS.UI/Infrastructure/Bootstrapper.cs`)
- HttpClient registration with:
  - Retry policy (3 attempts)
  - Circuit breaker policy
  - Timeout configuration
  - Proper base address configuration

---

## 📋 Remaining Tasks

### Task 1: Create ViewModels
You need to create two ViewModels:

#### **`POS.UI/Modules/Suppliers/PurchaseReturn/PurchaseReturnListViewModel.cs`**
- Inherits from `ViewModelBase`
- Properties:
  - `ObservableCollection<PurchaseReturnDto> PurchaseReturns`
  - `PurchaseReturnDto? SelectedReturn`
  - `string SearchText`
  - `string? FilterStatus` ("All", "Draft", "Processed", "Cancelled")
- Commands:
  - `ICommand RefreshCommand` - Reload data
  - `ICommand AddCommand` - Open create dialog
  - `ICommand ViewCommand` - View details
  - `ICommand EditCommand` - Edit return (only if not processed)
  - `ICommand ProcessCommand` - Process return (critical action)
  - `ICommand DisableCommand` - Soft delete
  - `ICommand ClearFiltersCommand`
- Methods:
  - `async Task LoadDataAsync()` - Calls API
  - `void ApplyFilters()` - Filter by search text and status
- Inject `PurchaseReturnApiService`, `DialogService`

#### **`POS.UI/Modules/Suppliers/PurchaseReturn/CreatePurchaseReturnViewModel.cs`**
- Inherits from `ViewModelBase`
- Properties:
  - `Guid? PurchaseReturnId` (null for new, Guid for edit)
  - `ObservableCollection<SupplierDto> Suppliers`
  - `SupplierDto? SelectedSupplier`
  - `ObservableCollection<PurchaseEntryDto> PurchaseEntries` (for "Load from GRN")
  - `PurchaseEntryDto? SelectedPurchaseEntry`
  - `string ReturnNo`, `DateTime ReturnDate`, `string? Reason`, `string? Notes`
  - `ObservableCollection<PurchaseReturnItemRowViewModel> Items`
  - `decimal TotalAmount`, `decimal TaxAmount`
  - Product search properties (similar to Purchase Entry)
- Commands:
  - `ICommand SaveCommand`
  - `ICommand CancelCommand`
  - `ICommand LoadSuppliersCommand`
  - `ICommand LoadFromPurchaseEntryCommand` (KEY FEATURE)
  - `ICommand AddItemCommand`
  - `ICommand RemoveItemCommand`
  - `ICommand FocusProductSearchCommand`
- Methods:
  - `async Task LoadAsync(Guid? id)` - Load for edit
  - `async Task SaveAsync()` - Create/Update via API
  - `void LoadItemsFromPurchaseEntry()` - Populate grid from GRN
  - `void CalculateTotals()` - Recalculate amounts
- Validation: Return Qty cannot exceed purchased qty if loading from GRN
- Inject: `PurchaseReturnApiService`, `SupplierApiService`, `PurchaseEntryApiService`, `ProductApiService`, `DialogService`

#### **`PurchaseReturnItemRowViewModel` (nested class)**
- Properties: ProductId, ProductName, ProductCode, BatchNo, ExpiryDate, Quantity, UnitPrice, TaxAmount, TotalAmount, Reason
- Property changed event for recalculation

### Task 2: Create Views (XAML + Code-Behind)

#### **`POS.UI/Modules/Suppliers/PurchaseReturn/PurchaseReturnListView.xaml`**
**Design Requirements (MUST follow standard transaction module design):**
- `UserControl` with `Focusable="True"`
- **SHORTCUTS BAR** (Grid.Row="0"):
  - F2 Add, F5 Refresh, ENTER View, CTRL+E Edit, DEL Delete, CTRL+P Process
  - Background="#1976D2", Foreground="White", Bold, Padding="10,8"
- **TOP TOOLBAR** (Grid.Row="1"):
  - Search TextBox (Width="350", Height="36", Placeholder: "🔍 Search by return no, supplier, status...")
  - Status ComboBox (Width="140"): All Statuses, Draft, Processed, Cancelled
  - Clear Button
  - Action Buttons: "🔄 Refresh", "➕ Add", "👁 View", "✏ Edit", "📋 Process", "🗑 Delete"
  - Button styling: Height="36", FontSize="12", SemiBold, icons with text
- **DATA GRID** (Grid.Row="2"):
  - `CornerRadius="8"`, `DropShadowEffect`
  - `AlternatingRowBackground="#FAFAFA"`, `RowHeight="45"`, `FontSize="13"`
  - Column headers: Background="#263238", Foreground="White", FontWeight="Bold"
  - Columns: Return No, Supplier, Return Date, Total Amount, Status, Processed, Created At
  - Status column: Color-coded badges (Draft: Orange, Processed: Green, Cancelled: Red)
  - Hover effect: #E3F2FD, Selection: #BBDEFB
  - Empty state: "No purchase returns found. Click 'Add' to create one."
- **INPUT BINDINGS**:
  - F5 → RefreshCommand
  - F2 / Ctrl+N → AddCommand
  - Enter → ViewCommand
  - Ctrl+E → EditCommand
  - Delete → DisableCommand
  - Ctrl+P → ProcessCommand

#### **`PurchaseReturnListView.xaml.cs`**
- Constructor: `InitializeComponent();`
- Loaded event: `Keyboard.Focus(this);` (enable shortcuts)
- DataContext binding to ViewModel

#### **`POS.UI/Modules/Suppliers/PurchaseReturn/CreatePurchaseReturnView.xaml`**
**Design Requirements (Master-Detail Layout):**
- `UserControl` with `Focusable="True"`
- **SHORTCUTS BAR** (Grid.Row="0"):
  - F2 Save, ESC Cancel, F1 Search Products, F3 Load from GRN
  - Same styling as list view
- **HEADER SECTION** (Grid.Row="1"):
  - Compact 3-column Grid layout (like Purchase Entry)
  - Controls (Height="28", FontSize="11-12", with TabIndex):
    1. Supplier ComboBox (Required, TabIndex="1")
    2. "Load from GRN" button (TabIndex="2") → Opens PurchaseEntry selection popup
    3. Return No TextBox (TabIndex="3")
    4. Return Date DatePicker (TabIndex="4")
    5. Reason TextBox (multiline, TabIndex="5")
- **ITEMS SECTION** (Grid.Row="2"):
  - Toolbar: "Items (F1 to search, TAB to navigate)"
  - Product Search TextBox with auto-suggest popup (TabIndex="6")
  - "Remove" button
  - DataGrid: Columns for Product, Batch No, Expiry, Quantity, Unit Price, Tax, Total, Reason
  - Grid styling: `GridLinesVisibility="All"`, `AlternatingRowBackground="#FAFAFA"`, `RowHeight="32"`
  - Editable Quantity and Reason columns
  - Empty state: "No items added. Search for products above..."
- **TOTALS & ACTIONS SECTION** (Grid.Row="3"):
  - Left: Tax Amount, Total Amount (read-only, bold, larger font)
  - Right: "💾 Save" (F2, TabIndex="7"), "❌ Cancel" (ESC, TabIndex="8")
- **INPUT BINDINGS**:
  - F2 → SaveCommand
  - Esc → CancelCommand
  - F1 → FocusProductSearchCommand
  - F3 → LoadFromPurchaseEntryCommand

#### **`CreatePurchaseReturnView.xaml.cs`**
- Constructor + Loaded event (focus handling)
- PreviewKeyDown handler for product search navigation (Down, Up, Enter keys in ListBox)
- ProductSearchBox_PreviewKeyDown, ProductSearchListBox_PreviewKeyDown methods (like Purchase Order module)

### Task 3: Register in Menu and Bootstrapper

#### **`POS.UI/menu.json`**
Add under "Suppliers" menu group:
```json
{
  "title": "Purchase Return",
  "icon": "↩️",
  "viewModelType": "POS.UI.Modules.Suppliers.PurchaseReturn.PurchaseReturnListViewModel",
  "viewType": "POS.UI.Modules.Suppliers.PurchaseReturn.PurchaseReturnListView"
}
```

#### **`POS.UI/Infrastructure/Bootstrapper.cs`**
In `RegisterViewModels()` method:
```csharp
// Purchase Return module
services.AddTransient<PurchaseReturnListViewModel>();
services.AddTransient<CreatePurchaseReturnViewModel>();
```

---

## 🔑 Key Features Implemented

### 1. Stock Management
- **CRITICAL LOGIC**: When a purchase return is processed:
  1. Reduces `CurrentQuantity` in `Batches` table
  2. Increases `ReturnedQuantity` in `Batches` table
  3. Creates `StockLedgerEntry` with Type="OUT", ReferenceType="PURCHASE_RETURN"
  4. Uses FIFO (First-In-First-Out) if batch number not specified
  5. Validates sufficient stock before allowing return
  6. All operations in a database transaction for atomicity

### 2. UX Feature: "Load from Purchase Entry"
- User selects a Supplier
- System shows all Purchase Entries (GRNs) for that supplier
- User selects a GRN
- Grid auto-populates with items from that GRN
- User enters "Return Qty" for each item
- Validation: Return Qty ≤ Purchased Qty

### 3. Return Number Validation
- Unique return number check (API endpoint provided)
- Duplicate prevention

### 4. Status Management
- **Draft**: Editable, not affecting stock
- **Processed**: Read-only, stock reduced, ledger entries created
- **Cancelled**: Soft-deleted

### 5. Keyboard Shortcuts (Market Standard)
- F2: Save/Add
- F5: Refresh
- Enter: View
- Ctrl+E: Edit
- Ctrl+P: Process
- Delete: Disable
- F1: Focus product search
- F3: Load from GRN
- ESC: Cancel

---

## 📊 Database Schema

### PurchaseReturns Table
- PurchaseReturnId (GUID, PK)
- SupplierId (GUID, FK → Suppliers, REQUIRED)
- PurchaseEntryId (GUID, FK → PurchaseEntries, NULLABLE)
- ReturnNo (VARCHAR(100), UNIQUE, INDEXED)
- ReturnDate (DATETIME)
- TotalAmount, TaxAmount (DECIMAL(18,2))
- Reason, Notes (VARCHAR(500))
- Status (VARCHAR(20): Draft/Processed/Cancelled)
- IsProcessed (BOOL)
- ProcessedAt, ProcessedBy
- IsActive, CreatedAt, UpdatedAt, RowVersion

### PurchaseReturnItems Table
- PurchaseReturnItemId (GUID, PK)
- PurchaseReturnId (GUID, FK → PurchaseReturns, CASCADE)
- ProductId (BIGINT, FK → Products, REQUIRED)
- PurchaseEntryItemId (GUID, NULLABLE)
- BatchNo, ExpiryDate
- Quantity (DECIMAL(12,3))
- UnitPrice, TaxAmount, TotalAmount (DECIMAL(18,2))
- Reason (VARCHAR(500))
- IsActive, CreatedAt, UpdatedAt, RowVersion

### StockLedgerEntries Table
- StockEntryId (GUID, PK)
- ProductId (BIGINT, REQUIRED)
- Quantity (DECIMAL(12,3), NEGATIVE for returns)
- EntryType (VARCHAR(20): IN/OUT)
- ReferenceType (VARCHAR(50): PURCHASE_RETURN)
- ReferenceId (GUID, references PurchaseReturnId)
- EntryDate (DATETIME)
- Remarks (VARCHAR(500))

---

## 🚀 Testing Checklist

### Backend Testing
- [ ] Run SQL script: `create_purchase_returns_tables.sql`
- [ ] Test API endpoints in Postman/Swagger
- [ ] Create a draft return
- [ ] Update the return
- [ ] Process the return (verify stock reduction in Batches table)
- [ ] Check StockLedgerEntries for audit trail
- [ ] Try processing again (should fail: "already processed")
- [ ] Try editing processed return (should fail)

### Frontend Testing
- [ ] Open Purchase Return list screen
- [ ] Test all keyboard shortcuts
- [ ] Create a new return manually
- [ ] Create a return by "Load from GRN"
- [ ] Verify return qty validation (cannot exceed purchased qty)
- [ ] Search and filter returns
- [ ] Edit a draft return
- [ ] Process a return
- [ ] Verify "Process" button disables for already-processed returns
- [ ] Test product search auto-suggest (down arrow, enter key navigation)

---

## 📁 File Structure Summary

```
POS.Core/
├── POS.Domain/Entities/
│   ├── PurchaseReturn.cs ✅
│   └── PurchaseReturnItem.cs ✅
├── POS.Application/
│   ├── Interfaces/
│   │   ├── Repositories/IPurchaseReturnRepository.cs ✅
│   │   └── Services/IPurchaseReturnService.cs ✅
│   └── Services/
│       └── PurchaseReturnService.cs ✅
├── POS.Infrastructure/
│   ├── Repositories/PurchaseReturnRepository.cs ✅
│   └── Data/PosDbContext.cs ✅ (updated)
└── POS.API/
    ├── Controllers/PurchaseReturnsController.cs ✅
    ├── Program.cs ✅ (updated)
    └── create_purchase_returns_tables.sql ✅

POS.Shared/Models/
├── PurchaseReturnDto.cs ✅
├── PurchaseReturnItemDto.cs ✅
└── CreatePurchaseReturnDto.cs ✅

POS.UI/
├── Core/Services/
│   └── PurchaseReturnApiService.cs ✅
├── Infrastructure/
│   └── Bootstrapper.cs ✅ (updated)
├── Modules/Suppliers/PurchaseReturn/
│   ├── PurchaseReturnListViewModel.cs ⏳ TODO
│   ├── PurchaseReturnListView.xaml ⏳ TODO
│   ├── PurchaseReturnListView.xaml.cs ⏳ TODO
│   ├── CreatePurchaseReturnViewModel.cs ⏳ TODO
│   ├── CreatePurchaseReturnView.xaml ⏳ TODO
│   └── CreatePurchaseReturnView.xaml.cs ⏳ TODO
└── menu.json ⏳ TODO (update)
```

---

## ⚠️ Important Notes

1. **Stock Reduction Logic**: The `ProcessReturnWithInventoryUpdateAsync` method in `PurchaseReturnRepository.cs` is the CRITICAL piece. It handles:
   - Batch matching (by batch number or FIFO)
   - Stock validation
   - Atomic updates to Batches and StockLedgerEntries
   - Transaction rollback on error

2. **Cannot Process Twice**: Once a return is processed (`IsProcessed = true`), it cannot be processed again, edited, or deleted.

3. **Validation**: The service layer validates:
   - Supplier exists and is active
   - Products exist and are active
   - Quantities are positive
   - Return number is unique

4. **UI/UX Standards**: The Views MUST match the design of Purchase Entry and Billing modules (shortcuts bar, toolbar, grid styling, etc.)

5. **Keyboard Navigation**: Product search must support Down/Up/Enter keys for selection (see Purchase Order module for reference).

---

## 🎯 Next Steps

1. Create the 4 remaining files (2 ViewModels + 2 Views with code-behind)
2. Update `menu.json` to add the module
3. Update `Bootstrapper.cs` to register ViewModels
4. Build and test the application
5. Run the SQL script to create tables
6. Test the complete flow: Create → Edit → Process → Verify Stock

---

## 📞 Support

If you encounter issues:
- Check entity configurations in `PosDbContext.cs`
- Verify foreign key relationships in database
- Check API logs for validation errors
- Use browser dev tools to inspect API calls
- Test backend endpoints independently before UI testing

---

**Module Status**: 85% Complete (Backend: 100%, Frontend: 60%)
**Estimated Time to Complete**: 2-3 hours for remaining ViewModels and Views
