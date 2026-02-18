-- =====================================================
-- SQL Script: Sales Return enhancements, Customer Transactions, Quotations
-- Run this against your existing POS database
-- =====================================================

-- 1. Enhance Returns table with new columns
ALTER TABLE `Returns` 
ADD COLUMN IF NOT EXISTS `ReturnDate` DATETIME DEFAULT CURRENT_TIMESTAMP,
ADD COLUMN IF NOT EXISTS `RefundMode` VARCHAR(20) DEFAULT 'Cash',
ADD COLUMN IF NOT EXISTS `CustomerId` VARCHAR(36) NULL,
ADD COLUMN IF NOT EXISTS `Status` VARCHAR(20) DEFAULT 'Draft',
ADD COLUMN IF NOT EXISTS `IsProcessed` TINYINT(1) DEFAULT 0;

CREATE INDEX IF NOT EXISTS `idx_returns_returndate` ON `Returns` (`ReturnDate`);

-- 2. Enhance ReturnItems table with new columns
ALTER TABLE `ReturnItems`
ADD COLUMN IF NOT EXISTS `ProductId` BIGINT DEFAULT 0,
ADD COLUMN IF NOT EXISTS `ProductName` VARCHAR(200) DEFAULT '',
ADD COLUMN IF NOT EXISTS `RefundPrice` DECIMAL(18,2) DEFAULT 0,
ADD COLUMN IF NOT EXISTS `IsRestockable` TINYINT(1) DEFAULT 1,
ADD COLUMN IF NOT EXISTS `Reason` VARCHAR(500) NULL;

-- 3. Create CustomerTransactions table
CREATE TABLE IF NOT EXISTS `CustomerTransactions` (
    `CustomerTransactionId` VARCHAR(36) NOT NULL PRIMARY KEY,
    `CustomerId` VARCHAR(36) NOT NULL,
    `TransactionDate` DATETIME NOT NULL,
    `TransactionType` VARCHAR(20) NOT NULL,
    `ReferenceId` VARCHAR(36) NULL,
    `ReferenceNo` VARCHAR(100) NULL,
    `DebitAmount` DECIMAL(18,2) NOT NULL DEFAULT 0,
    `CreditAmount` DECIMAL(18,2) NOT NULL DEFAULT 0,
    `Balance` DECIMAL(18,2) NOT NULL DEFAULT 0,
    `Description` VARCHAR(500) NULL,
    `PaymentMode` VARCHAR(20) NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NULL,
    `RowVersion` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX `IX_CustomerTransactions_CustomerId` (`CustomerId`),
    INDEX `IX_CustomerTransactions_TransactionDate` (`TransactionDate`),
    INDEX `IX_CustomerTransactions_TransactionType` (`TransactionType`)
);

-- 4. Create Quotations table
CREATE TABLE IF NOT EXISTS `Quotations` (
    `QuotationId` VARCHAR(36) NOT NULL PRIMARY KEY,
    `QuotationNumber` VARCHAR(50) NOT NULL,
    `QuotationDate` DATETIME NOT NULL,
    `ValidUntil` DATETIME NULL,
    `CustomerId` VARCHAR(36) NULL,
    `CustomerName` VARCHAR(200) NULL,
    `CustomerPhone` VARCHAR(20) NULL,
    `Status` VARCHAR(20) NOT NULL DEFAULT 'Open',
    `Subtotal` DECIMAL(18,2) NOT NULL DEFAULT 0,
    `DiscountAmount` DECIMAL(18,2) NOT NULL DEFAULT 0,
    `TaxAmount` DECIMAL(18,2) NOT NULL DEFAULT 0,
    `TotalAmount` DECIMAL(18,2) NOT NULL DEFAULT 0,
    `Notes` VARCHAR(500) NULL,
    `TermsAndConditions` VARCHAR(1000) NULL,
    `ConvertedSaleId` BIGINT NULL,
    `ConvertedAt` DATETIME NULL,
    `ConvertedBy` VARCHAR(100) NULL,
    `CreatedBy` VARCHAR(100) NOT NULL DEFAULT 'System',
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NULL,
    `RowVersion` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    UNIQUE INDEX `UX_Quotations_QuotationNumber` (`QuotationNumber`),
    INDEX `IX_Quotations_QuotationDate` (`QuotationDate`),
    INDEX `IX_Quotations_CustomerId` (`CustomerId`),
    INDEX `IX_Quotations_Status` (`Status`)
);

-- 5. Create QuotationItems table
CREATE TABLE IF NOT EXISTS `QuotationItems` (
    `QuotationItemId` VARCHAR(36) NOT NULL PRIMARY KEY,
    `QuotationId` VARCHAR(36) NOT NULL,
    `ProductId` BIGINT NOT NULL,
    `ProductName` VARCHAR(200) NOT NULL,
    `SKU` VARCHAR(100) NOT NULL,
    `HSNCode` VARCHAR(20) NULL,
    `Quantity` DECIMAL(18,3) NOT NULL DEFAULT 0,
    `UnitName` VARCHAR(50) NULL,
    `UnitPrice` DECIMAL(18,2) NOT NULL DEFAULT 0,
    `DiscountPercent` DECIMAL(5,2) NOT NULL DEFAULT 0,
    `DiscountAmount` DECIMAL(18,2) NOT NULL DEFAULT 0,
    `TaxRate` DECIMAL(5,2) NOT NULL DEFAULT 0,
    `TaxAmount` DECIMAL(18,2) NOT NULL DEFAULT 0,
    `TotalAmount` DECIMAL(18,2) NOT NULL DEFAULT 0,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NULL,
    `RowVersion` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX `IX_QuotationItems_QuotationId` (`QuotationId`),
    INDEX `IX_QuotationItems_ProductId` (`ProductId`),
    CONSTRAINT `FK_QuotationItems_Quotations` FOREIGN KEY (`QuotationId`) REFERENCES `Quotations`(`QuotationId`) ON DELETE CASCADE,
    CONSTRAINT `FK_QuotationItems_Products` FOREIGN KEY (`ProductId`) REFERENCES `Products`(`ProductId`) ON DELETE RESTRICT
);
