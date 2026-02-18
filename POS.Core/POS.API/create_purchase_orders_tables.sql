-- Create PurchaseOrders table
CREATE TABLE `PurchaseOrders` (
    `PurchaseOrderId` CHAR(36) NOT NULL COLLATE ascii_general_ci,
    `SupplierId` CHAR(36) NOT NULL COLLATE ascii_general_ci,
    `OrderDate` DATETIME NOT NULL,
    `ExpectedDeliveryDate` DATETIME NULL,
    `Status` VARCHAR(20) NOT NULL,
    `TotalAmount` DECIMAL(18,2) NOT NULL,
    `ReferenceNo` VARCHAR(100) NULL,
    `Notes` VARCHAR(500) NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NULL,
    `RowVersion` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`PurchaseOrderId`),
    INDEX `IX_PurchaseOrders_ReferenceNo` (`ReferenceNo`),
    INDEX `IX_PurchaseOrders_SupplierId` (`SupplierId`),
    INDEX `IX_PurchaseOrders_OrderDate` (`OrderDate`),
    CONSTRAINT `FK_PurchaseOrders_Suppliers_SupplierId` FOREIGN KEY (`SupplierId`) 
        REFERENCES `Suppliers` (`SupplierId`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Create PurchaseOrderItems table
CREATE TABLE `PurchaseOrderItems` (
    `PurchaseOrderItemId` CHAR(36) NOT NULL COLLATE ascii_general_ci,
    `PurchaseOrderId` CHAR(36) NOT NULL COLLATE ascii_general_ci,
    `ProductId` BIGINT NOT NULL,
    `Quantity` DECIMAL(12,3) NOT NULL,
    `UnitPrice` DECIMAL(18,2) NOT NULL,
    `TaxAmount` DECIMAL(18,2) NOT NULL,
    `TotalAmount` DECIMAL(18,2) NOT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NULL,
    `RowVersion` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`PurchaseOrderItemId`),
    INDEX `IX_PurchaseOrderItems_PurchaseOrderId` (`PurchaseOrderId`),
    INDEX `IX_PurchaseOrderItems_ProductId` (`ProductId`),
    CONSTRAINT `FK_PurchaseOrderItems_PurchaseOrders_PurchaseOrderId` FOREIGN KEY (`PurchaseOrderId`) 
        REFERENCES `PurchaseOrders` (`PurchaseOrderId`) ON DELETE CASCADE,
    CONSTRAINT `FK_PurchaseOrderItems_Products_ProductId` FOREIGN KEY (`ProductId`) 
        REFERENCES `Products` (`ProductId`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Insert migration history record
INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260205_AddPurchaseOrderTables', '8.0.0');
