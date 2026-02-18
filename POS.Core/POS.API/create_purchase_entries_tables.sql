-- ========================================
-- PURCHASE ENTRY (GRN) MODULE - Database Migration
-- Critical for Inventory Management
-- ========================================

-- Create PurchaseEntries table
CREATE TABLE `PurchaseEntries` (
    `PurchaseEntryId` CHAR(36) NOT NULL COLLATE ascii_general_ci,
    `SupplierId` CHAR(36) NOT NULL COLLATE ascii_general_ci,
    `PurchaseOrderId` CHAR(36) NULL COLLATE ascii_general_ci,
    `InvoiceNo` VARCHAR(100) NOT NULL,
    `InvoiceDate` DATETIME NOT NULL,
    `ReceivedDate` DATETIME NOT NULL,
    `TotalAmount` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `TaxAmount` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `Notes` VARCHAR(500) NULL,
    `IsProcessed` TINYINT(1) NOT NULL DEFAULT 0,
    `ProcessedAt` DATETIME NULL,
    `ProcessedBy` VARCHAR(100) NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NULL,
    `RowVersion` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`PurchaseEntryId`),
    INDEX `IX_PurchaseEntries_InvoiceNo` (`InvoiceNo`),
    INDEX `IX_PurchaseEntries_SupplierId` (`SupplierId`),
    INDEX `IX_PurchaseEntries_PurchaseOrderId` (`PurchaseOrderId`),
    INDEX `IX_PurchaseEntries_ReceivedDate` (`ReceivedDate`),
    INDEX `IX_PurchaseEntries_IsProcessed` (`IsProcessed`),
    CONSTRAINT `FK_PurchaseEntries_Suppliers_SupplierId` FOREIGN KEY (`SupplierId`) 
        REFERENCES `Suppliers` (`SupplierId`) ON DELETE RESTRICT,
    CONSTRAINT `FK_PurchaseEntries_PurchaseOrders_PurchaseOrderId` FOREIGN KEY (`PurchaseOrderId`) 
        REFERENCES `PurchaseOrders` (`PurchaseOrderId`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci
COMMENT='Purchase Entry (GRN) - Goods Receipt Notes for inventory management';

-- Create PurchaseEntryItems table
CREATE TABLE `PurchaseEntryItems` (
    `PurchaseEntryItemId` CHAR(36) NOT NULL COLLATE ascii_general_ci,
    `PurchaseEntryId` CHAR(36) NOT NULL COLLATE ascii_general_ci,
    `ProductId` BIGINT NOT NULL,
    `BatchNo` VARCHAR(100) NULL,
    `ExpiryDate` DATE NULL,
    `Quantity` DECIMAL(12,3) NOT NULL,
    `CostPrice` DECIMAL(18,2) NOT NULL,
    `SellingPrice` DECIMAL(18,2) NOT NULL,
    `MRP` DECIMAL(18,2) NOT NULL,
    `TaxAmount` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `TotalAmount` DECIMAL(18,2) NOT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NULL,
    `RowVersion` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`PurchaseEntryItemId`),
    INDEX `IX_PurchaseEntryItems_PurchaseEntryId` (`PurchaseEntryId`),
    INDEX `IX_PurchaseEntryItems_ProductId` (`ProductId`),
    INDEX `IX_PurchaseEntryItems_BatchNo` (`BatchNo`),
    CONSTRAINT `FK_PurchaseEntryItems_PurchaseEntries_PurchaseEntryId` FOREIGN KEY (`PurchaseEntryId`) 
        REFERENCES `PurchaseEntries` (`PurchaseEntryId`) ON DELETE CASCADE,
    CONSTRAINT `FK_PurchaseEntryItems_Products_ProductId` FOREIGN KEY (`ProductId`) 
        REFERENCES `Products` (`ProductId`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci
COMMENT='Purchase Entry Items - Individual products received with batch, expiry, and pricing info';

-- Insert migration history record
INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260205_AddPurchaseEntryTables', '8.0.0');

-- ========================================
-- IMPORTANT NOTES FOR PRODUCTION USE:
-- ========================================
-- 
-- 1. CRITICAL: IsProcessed Flag
--    - When IsProcessed = FALSE: Entry can be edited or deleted
--    - When IsProcessed = TRUE: Entry has updated inventory and CANNOT be deleted
--    - Processing updates: StockSummary, StockLedgerEntry, Product prices, PurchaseOrder status
--
-- 2. Data Integrity:
--    - All inventory updates happen in a database transaction (atomic)
--    - StockLedgerEntry provides complete audit trail
--    - Cannot delete processed entries (inventory already updated)
--
-- 3. Batch and Expiry Tracking:
--    - BatchNo: Track product batches for recall/traceability
--    - ExpiryDate: Track expiration dates for perishable items
--    - Both fields are optional (NULL allowed)
--
-- 4. Price Management:
--    - CostPrice: What you paid to the supplier
--    - SellingPrice: Your selling price
--    - MRP: Maximum Retail Price
--    - All prices can be updated during GRN entry
--    - Optional: Update Product master prices when processing
--
-- 5. PurchaseOrder Link:
--    - PurchaseOrderId is OPTIONAL (can be NULL)
--    - NULL = Direct purchase without PO
--    - NOT NULL = Linked to a Purchase Order
--    - When processed, linked PO status automatically updated to 'Received'
--
-- 6. Performance Considerations:
--    - Index on IsProcessed for fast filtering
--    - Index on BatchNo for quick batch lookups
--    - Index on SupplierId and ReceivedDate for reporting
--
-- 7. Workflow:
--    a. Create Purchase Entry (IsProcessed = FALSE)
--    b. Add items with quantities, prices, batch, expiry
--    c. Save entry (can edit/delete at this point)
--    d. Click PROCESS button (IsProcessed = TRUE, ProcessedAt = NOW)
--    e. Inventory updated automatically
--    f. Cannot edit or delete after processing
--
-- ========================================

SELECT 'Purchase Entry (GRN) tables created successfully!' AS Status;
SELECT 'NEXT STEP: Verify StockSummary and StockLedgerEntry tables exist' AS Action;
