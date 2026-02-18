-- SQL Script: Create SupplierPayments and SupplierTransactions Tables
-- Database: POS (MySQL)
-- Description: Creates tables for Supplier Payment module and Ledger

-- =============================================
-- Table: SupplierPayments
-- Description: Stores supplier payment records
-- =============================================
CREATE TABLE IF NOT EXISTS `SupplierPayments` (
    `SupplierPaymentId` CHAR(36) NOT NULL,
    `SupplierId` CHAR(36) NOT NULL,
    `PaymentDate` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `Amount` DECIMAL(18, 2) NOT NULL,
    `PaymentMode` VARCHAR(20) NOT NULL DEFAULT 'Cash',
    `ReferenceNo` VARCHAR(100) NULL,
    `BankName` VARCHAR(100) NULL,
    `Remarks` VARCHAR(500) NULL,
    `PaymentNo` VARCHAR(50) NOT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    `RowVersion` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`SupplierPaymentId`),
    CONSTRAINT `FK_SupplierPayments_Suppliers` FOREIGN KEY (`SupplierId`) 
        REFERENCES `Suppliers` (`SupplierId`) ON DELETE RESTRICT,
    CONSTRAINT `UX_SupplierPayments_PaymentNo` UNIQUE (`PaymentNo`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Indexes for SupplierPayments
CREATE INDEX `IX_SupplierPayments_SupplierId` ON `SupplierPayments` (`SupplierId`);
CREATE INDEX `IX_SupplierPayments_PaymentDate` ON `SupplierPayments` (`PaymentDate`);
CREATE INDEX `IX_SupplierPayments_PaymentMode` ON `SupplierPayments` (`PaymentMode`);
CREATE INDEX `IX_SupplierPayments_IsActive` ON `SupplierPayments` (`IsActive`);

-- =============================================
-- Table: SupplierTransactions
-- Description: Supplier ledger - tracks all financial transactions
-- =============================================
CREATE TABLE IF NOT EXISTS `SupplierTransactions` (
    `SupplierTransactionId` CHAR(36) NOT NULL,
    `SupplierId` CHAR(36) NOT NULL,
    `TransactionDate` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `TransactionType` VARCHAR(50) NOT NULL,
    `ReferenceId` CHAR(36) NULL,
    `ReferenceNo` VARCHAR(100) NULL,
    `DebitAmount` DECIMAL(18, 2) NOT NULL DEFAULT 0.00,
    `CreditAmount` DECIMAL(18, 2) NOT NULL DEFAULT 0.00,
    `Balance` DECIMAL(18, 2) NOT NULL DEFAULT 0.00,
    `Description` VARCHAR(500) NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    `RowVersion` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`SupplierTransactionId`),
    CONSTRAINT `FK_SupplierTransactions_Suppliers` FOREIGN KEY (`SupplierId`) 
        REFERENCES `Suppliers` (`SupplierId`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Indexes for SupplierTransactions
CREATE INDEX `IX_SupplierTransactions_SupplierId` ON `SupplierTransactions` (`SupplierId`);
CREATE INDEX `IX_SupplierTransactions_TransactionDate` ON `SupplierTransactions` (`TransactionDate`);
CREATE INDEX `IX_SupplierTransactions_TransactionType` ON `SupplierTransactions` (`TransactionType`);
CREATE INDEX `IX_SupplierTransactions_ReferenceId` ON `SupplierTransactions` (`ReferenceId`);

-- =============================================
-- Comments:
-- Transaction Types:
--   'Purchase' - Credit entry when purchase is made (amount owed to supplier)
--   'Return'   - Debit entry when goods are returned to supplier
--   'Payment'  - Debit entry when payment is made to supplier
-- 
-- Balance Calculation:
--   Balance = Previous Balance + CreditAmount - DebitAmount
--   Positive Balance = Amount owed to supplier
--   Negative Balance = Advance payment (supplier owes us)
-- =============================================
