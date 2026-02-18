-- =====================================================
-- Add ONLY Batches Table (Clean Migration)
-- =====================================================

CREATE TABLE IF NOT EXISTS `Batches` (
    `BatchId` CHAR(36) NOT NULL COLLATE ascii_general_ci,
    `ProductId` BIGINT NOT NULL,
    `BatchNo` VARCHAR(100) NOT NULL,
    `ExpiryDate` DATE NULL,
    `ManufactureDate` DATE NULL,
    `CostPrice` DECIMAL(18,2) NOT NULL,
    `SellingPrice` DECIMAL(18,2) NOT NULL,
    `MRP` DECIMAL(18,2) NOT NULL,
    `ReceivedQuantity` DECIMAL(12,3) NOT NULL,
    `CurrentQuantity` DECIMAL(12,3) NOT NULL,
    `AllocatedQuantity` DECIMAL(12,3) NOT NULL,
    `SoldQuantity` DECIMAL(12,3) NOT NULL,
    `ReturnedQuantity` DECIMAL(12,3) NOT NULL,
    `AdjustedQuantity` DECIMAL(12,3) NOT NULL,
    `PurchaseEntryId` CHAR(36) NULL COLLATE ascii_general_ci,
    `PurchaseEntryItemId` CHAR(36) NULL COLLATE ascii_general_ci,
    `SupplierId` CHAR(36) NOT NULL COLLATE ascii_general_ci,
    `LocationCode` VARCHAR(50) NULL,
    `BinLocation` VARCHAR(50) NULL,
    `ReorderLevel` DECIMAL(12,3) NOT NULL,
    `ReceivedDate` DATETIME NOT NULL,
    `ReceivedBy` VARCHAR(100) NULL,
    `LastTransactionDate` DATETIME NULL,
    `IsActive` TINYINT(1) NOT NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NULL,
    `RowVersion` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    PRIMARY KEY (`BatchId`),
    
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
    
    INDEX `IX_Batches_ProductId` (`ProductId`),
    INDEX `IX_Batches_BatchNo` (`BatchNo`),
    INDEX `IX_Batches_ExpiryDate` (`ExpiryDate`),
    INDEX `IX_Batches_PurchaseEntryId` (`PurchaseEntryId`),
    INDEX `IX_Batches_PurchaseEntryItemId` (`PurchaseEntryItemId`),
    INDEX `IX_Batches_SupplierId` (`SupplierId`),
    INDEX `IX_Batches_Product_Batch` (`ProductId`, `BatchNo`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Verify table was created
SELECT 'Batches table created successfully!' AS Status;
