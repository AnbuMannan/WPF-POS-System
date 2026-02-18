-- =====================================================
-- Purchase Returns Module - Database Schema
-- Market-Standard POS System
-- =====================================================

-- Create PurchaseReturns Table
CREATE TABLE IF NOT EXISTS `PurchaseReturns` (
    `PurchaseReturnId` CHAR(36) NOT NULL,
    `SupplierId` CHAR(36) NOT NULL,
    `PurchaseEntryId` CHAR(36) NULL,
    `ReturnNo` VARCHAR(100) NOT NULL,
    `ReturnDate` DATETIME NOT NULL,
    `TotalAmount` DECIMAL(18,2) NOT NULL DEFAULT 0,
    `TaxAmount` DECIMAL(18,2) NOT NULL DEFAULT 0,
    `Reason` VARCHAR(500) NULL,
    `Notes` VARCHAR(500) NULL,
    `Status` VARCHAR(20) NOT NULL DEFAULT 'Draft',
    `IsProcessed` TINYINT(1) NOT NULL DEFAULT 0,
    `ProcessedAt` DATETIME NULL,
    `ProcessedBy` VARCHAR(100) NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NULL,
    `RowVersion` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`PurchaseReturnId`),
    INDEX `IX_PurchaseReturns_ReturnNo` (`ReturnNo`),
    INDEX `IX_PurchaseReturns_SupplierId` (`SupplierId`),
    INDEX `IX_PurchaseReturns_PurchaseEntryId` (`PurchaseEntryId`),
    INDEX `IX_PurchaseReturns_ReturnDate` (`ReturnDate`),
    CONSTRAINT `FK_PurchaseReturns_Suppliers` 
        FOREIGN KEY (`SupplierId`) 
        REFERENCES `Suppliers` (`SupplierId`) 
        ON DELETE RESTRICT,
    CONSTRAINT `FK_PurchaseReturns_PurchaseEntries` 
        FOREIGN KEY (`PurchaseEntryId`) 
        REFERENCES `PurchaseEntries` (`PurchaseEntryId`) 
        ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Create PurchaseReturnItems Table
CREATE TABLE IF NOT EXISTS `PurchaseReturnItems` (
    `PurchaseReturnItemId` CHAR(36) NOT NULL,
    `PurchaseReturnId` CHAR(36) NOT NULL,
    `ProductId` BIGINT NOT NULL,
    `PurchaseEntryItemId` CHAR(36) NULL,
    `BatchNo` VARCHAR(100) NULL,
    `ExpiryDate` DATE NULL,
    `Quantity` DECIMAL(12,3) NOT NULL DEFAULT 0,
    `UnitPrice` DECIMAL(18,2) NOT NULL DEFAULT 0,
    `TaxAmount` DECIMAL(18,2) NOT NULL DEFAULT 0,
    `TotalAmount` DECIMAL(18,2) NOT NULL DEFAULT 0,
    `Reason` VARCHAR(500) NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NULL,
    `RowVersion` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`PurchaseReturnItemId`),
    INDEX `IX_PurchaseReturnItems_PurchaseReturnId` (`PurchaseReturnId`),
    INDEX `IX_PurchaseReturnItems_ProductId` (`ProductId`),
    INDEX `IX_PurchaseReturnItems_BatchNo` (`BatchNo`),
    CONSTRAINT `FK_PurchaseReturnItems_PurchaseReturns` 
        FOREIGN KEY (`PurchaseReturnId`) 
        REFERENCES `PurchaseReturns` (`PurchaseReturnId`) 
        ON DELETE CASCADE,
    CONSTRAINT `FK_PurchaseReturnItems_Products` 
        FOREIGN KEY (`ProductId`) 
        REFERENCES `Products` (`ProductId`) 
        ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Create StockLedgerEntries Table (if not exists)
CREATE TABLE IF NOT EXISTS `StockLedgerEntries` (
    `StockEntryId` CHAR(36) NOT NULL,
    `ProductId` BIGINT NOT NULL COMMENT 'References Products.ProductId',
    `Quantity` DECIMAL(12,3) NOT NULL DEFAULT 0,
    `EntryType` VARCHAR(20) NOT NULL COMMENT 'IN, OUT, ADJUSTMENT, PURCHASE_RETURN',
    `ReferenceType` VARCHAR(50) NOT NULL COMMENT 'PURCHASE_ENTRY, SALE, PURCHASE_RETURN, ADJUSTMENT',
    `ReferenceId` CHAR(36) NULL,
    `EntryDate` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `Remarks` VARCHAR(500) NULL,
    PRIMARY KEY (`StockEntryId`),
    INDEX `IX_StockLedgerEntries_ProductId` (`ProductId`),
    INDEX `IX_StockLedgerEntries_ReferenceId` (`ReferenceId`),
    INDEX `IX_StockLedgerEntries_EntryDate` (`EntryDate`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- Sample Data / Test Queries (Optional)
-- =====================================================

-- Check tables created successfully
-- SELECT TABLE_NAME, TABLE_ROWS, CREATE_TIME 
-- FROM information_schema.TABLES 
-- WHERE TABLE_SCHEMA = DATABASE() 
-- AND TABLE_NAME IN ('PurchaseReturns', 'PurchaseReturnItems', 'StockLedgerEntries');

-- Grant permissions if needed
-- GRANT SELECT, INSERT, UPDATE, DELETE ON PurchaseReturns TO 'pos_user'@'localhost';
-- GRANT SELECT, INSERT, UPDATE, DELETE ON PurchaseReturnItems TO 'pos_user'@'localhost';
-- GRANT SELECT, INSERT, UPDATE, DELETE ON StockLedgerEntries TO 'pos_user'@'localhost';
