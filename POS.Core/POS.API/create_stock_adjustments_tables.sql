-- SQL Script: Create StockAdjustments Tables
-- Database: POS (MySQL)
-- Description: Creates tables for Stock Adjustment module

-- =============================================
-- Table: StockAdjustments
-- Description: Header table for stock adjustments
-- =============================================
CREATE TABLE IF NOT EXISTS `StockAdjustments` (
    `StockAdjustmentId` CHAR(36) NOT NULL,
    `ReferenceNo` VARCHAR(50) NOT NULL,
    `AdjustmentDate` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `AdjustedBy` VARCHAR(100) NOT NULL,
    `Reason` VARCHAR(50) NOT NULL COMMENT 'Damage, Theft, Expiry, Correction, Other',
    `Status` VARCHAR(20) NOT NULL DEFAULT 'Draft' COMMENT 'Draft, Approved, Cancelled',
    `Remarks` VARCHAR(500) NULL,
    `ApprovedAt` DATETIME NULL,
    `ApprovedBy` VARCHAR(100) NULL,
    `TotalValue` DECIMAL(18, 2) NOT NULL DEFAULT 0.00,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    `RowVersion` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`StockAdjustmentId`),
    CONSTRAINT `UX_StockAdjustments_ReferenceNo` UNIQUE (`ReferenceNo`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Indexes for StockAdjustments
CREATE INDEX `IX_StockAdjustments_AdjustmentDate` ON `StockAdjustments` (`AdjustmentDate`);
CREATE INDEX `IX_StockAdjustments_Reason` ON `StockAdjustments` (`Reason`);
CREATE INDEX `IX_StockAdjustments_Status` ON `StockAdjustments` (`Status`);
CREATE INDEX `IX_StockAdjustments_IsActive` ON `StockAdjustments` (`IsActive`);

-- =============================================
-- Table: StockAdjustmentItems
-- Description: Line items for stock adjustments
-- =============================================
CREATE TABLE IF NOT EXISTS `StockAdjustmentItems` (
    `StockAdjustmentItemId` CHAR(36) NOT NULL,
    `StockAdjustmentId` CHAR(36) NOT NULL,
    `ProductId` BIGINT NOT NULL,
    `BatchNo` VARCHAR(50) NULL,
    `Quantity` DECIMAL(12, 3) NOT NULL COMMENT 'Positive=increase, Negative=decrease',
    `CurrentStock` DECIMAL(12, 3) NOT NULL DEFAULT 0.000 COMMENT 'Stock at time of adjustment',
    `CostPrice` DECIMAL(18, 2) NOT NULL DEFAULT 0.00,
    `TotalValue` DECIMAL(18, 2) NOT NULL DEFAULT 0.00,
    `Remarks` VARCHAR(500) NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    `RowVersion` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`StockAdjustmentItemId`),
    CONSTRAINT `FK_StockAdjustmentItems_StockAdjustments` FOREIGN KEY (`StockAdjustmentId`) 
        REFERENCES `StockAdjustments` (`StockAdjustmentId`) ON DELETE CASCADE,
    CONSTRAINT `FK_StockAdjustmentItems_Products` FOREIGN KEY (`ProductId`) 
        REFERENCES `Products` (`ProductId`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Indexes for StockAdjustmentItems
CREATE INDEX `IX_StockAdjustmentItems_StockAdjustmentId` ON `StockAdjustmentItems` (`StockAdjustmentId`);
CREATE INDEX `IX_StockAdjustmentItems_ProductId` ON `StockAdjustmentItems` (`ProductId`);

-- =============================================
-- Comments:
-- Reason Values:
--   'Damage'     - Stock damaged and unusable
--   'Theft'      - Stock stolen/missing
--   'Expiry'     - Stock expired
--   'Correction' - Inventory count correction (can be +/-)
--   'Other'      - Other reasons
--
-- Status Values:
--   'Draft'     - Created but not yet processed
--   'Approved'  - Processed and inventory updated
--   'Cancelled' - Adjustment cancelled (draft only)
--
-- Quantity:
--   Positive = Stock increase (rare, mostly for corrections)
--   Negative = Stock decrease (damage, theft, expiry)
-- =============================================
