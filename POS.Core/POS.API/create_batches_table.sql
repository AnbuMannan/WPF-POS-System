-- =====================================================
-- Batches Table Schema for MySQL
-- Market-Standard POS System - Batch-Level Stock Management
-- =====================================================

CREATE TABLE IF NOT EXISTS `Batches` (
    -- Primary Key
    `BatchId` CHAR(36) NOT NULL COLLATE ascii_general_ci,
    
    -- Product Information
    `ProductId` BIGINT NOT NULL,
    `BatchNo` VARCHAR(100) NOT NULL,
    
    -- Date Information
    `ExpiryDate` DATE NULL,
    `ManufactureDate` DATE NULL,
    
    -- Pricing Information
    `CostPrice` DECIMAL(18,2) NOT NULL,
    `SellingPrice` DECIMAL(18,2) NOT NULL,
    `MRP` DECIMAL(18,2) NOT NULL,
    
    -- Stock Quantity Management
    `ReceivedQuantity` DECIMAL(12,3) NOT NULL,
    `CurrentQuantity` DECIMAL(12,3) NOT NULL,
    `AllocatedQuantity` DECIMAL(12,3) NOT NULL,
    `SoldQuantity` DECIMAL(12,3) NOT NULL,
    `ReturnedQuantity` DECIMAL(12,3) NOT NULL,
    `AdjustedQuantity` DECIMAL(12,3) NOT NULL,
    
    -- Purchase Entry References
    `PurchaseEntryId` CHAR(36) NULL COLLATE ascii_general_ci,
    `PurchaseEntryItemId` CHAR(36) NULL COLLATE ascii_general_ci,
    
    -- Supplier Information
    `SupplierId` CHAR(36) NOT NULL COLLATE ascii_general_ci,
    
    -- Location Management
    `LocationCode` VARCHAR(50) NULL,
    `BinLocation` VARCHAR(50) NULL,
    
    -- Reorder Management
    `ReorderLevel` DECIMAL(12,3) NOT NULL,
    
    -- Audit Fields
    `ReceivedDate` DATETIME NOT NULL,
    `ReceivedBy` VARCHAR(100) NULL,
    `LastTransactionDate` DATETIME NULL,
    `IsActive` TINYINT(1) NOT NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NULL,
    `RowVersion` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Primary Key Constraint
    PRIMARY KEY (`BatchId`),
    
    -- Foreign Key Constraints
    CONSTRAINT `FK_Batches_Products_ProductId` 
        FOREIGN KEY (`ProductId`) 
        REFERENCES `Products` (`ProductId`) 
        ON DELETE RESTRICT,
    
    CONSTRAINT `FK_Batches_PurchaseEntries_PurchaseEntryId` 
        FOREIGN KEY (`PurchaseEntryId`) 
        REFERENCES `PurchaseEntries` (`PurchaseEntryId`) 
        ON DELETE SET NULL,
    
    CONSTRAINT `FK_Batches_PurchaseEntryItems_PurchaseEntryItemId` 
        FOREIGN KEY (`PurchaseEntryItemId`) 
        REFERENCES `PurchaseEntryItems` (`PurchaseEntryItemId`) 
        ON DELETE SET NULL,
    
    CONSTRAINT `FK_Batches_Suppliers_SupplierId` 
        FOREIGN KEY (`SupplierId`) 
        REFERENCES `Suppliers` (`SupplierId`) 
        ON DELETE RESTRICT,
    
    -- Indices for Performance
    INDEX `IX_Batches_ProductId` (`ProductId`),
    INDEX `IX_Batches_BatchNo` (`BatchNo`),
    INDEX `IX_Batches_ExpiryDate` (`ExpiryDate`),
    INDEX `IX_Batches_PurchaseEntryId` (`PurchaseEntryId`),
    INDEX `IX_Batches_PurchaseEntryItemId` (`PurchaseEntryItemId`),
    INDEX `IX_Batches_SupplierId` (`SupplierId`),
    INDEX `IX_Batches_Product_Batch` (`ProductId`, `BatchNo`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- =====================================================
-- Column Descriptions
-- =====================================================
-- BatchId              : Unique identifier for the batch (GUID)
-- ProductId            : Reference to the product
-- BatchNo              : Batch/Lot number for tracking
-- ExpiryDate           : Product expiry date (NULL if non-perishable)
-- ManufactureDate      : Manufacturing date (NULL if not applicable)
-- CostPrice            : Purchase cost price per unit
-- SellingPrice         : Default selling price per unit
-- MRP                  : Maximum Retail Price
-- ReceivedQuantity     : Initial quantity received in this batch
-- CurrentQuantity      : Current available quantity (updated with sales/returns/adjustments)
-- AllocatedQuantity    : Quantity reserved for orders but not yet sold
-- SoldQuantity         : Total quantity sold from this batch
-- ReturnedQuantity     : Total quantity returned to stock
-- AdjustedQuantity     : Manual inventory adjustments (+ or -)
-- PurchaseEntryId      : Link to the purchase entry (GRN) that created this batch
-- PurchaseEntryItemId  : Link to the specific purchase entry item
-- SupplierId           : Supplier from whom this batch was purchased
-- LocationCode         : Warehouse/location code where batch is stored
-- BinLocation          : Specific bin/rack location within warehouse
-- ReorderLevel         : Minimum quantity before reorder alert
-- ReceivedDate         : Date when batch was received
-- ReceivedBy           : User who processed the batch
-- LastTransactionDate  : Last time this batch was modified (sale/return/adjustment)
-- IsActive             : Soft delete flag
-- CreatedAt            : Record creation timestamp
-- UpdatedAt            : Record last update timestamp
-- RowVersion           : Concurrency control timestamp

-- =====================================================
-- Usage Notes
-- =====================================================
-- 1. AvailableQuantity (computed): CurrentQuantity - AllocatedQuantity
-- 2. IsExpired (computed): ExpiryDate < CURRENT_DATE
-- 3. IsLowStock (computed): CurrentQuantity <= ReorderLevel
-- 4. FIFO: Order by ReceivedDate ASC for First-In-First-Out
-- 5. FEFO: Order by ExpiryDate ASC for First-Expiry-First-Out
-- 6. Stock Query: SELECT SUM(CurrentQuantity) FROM Batches WHERE ProductId = ? AND IsActive = 1
-- 7. Available Stock: SELECT SUM(CurrentQuantity - AllocatedQuantity) FROM Batches WHERE ProductId = ? AND IsActive = 1
