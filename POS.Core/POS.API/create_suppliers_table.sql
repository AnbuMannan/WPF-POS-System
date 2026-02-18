-- Create Suppliers table
CREATE TABLE `Suppliers` (
    `SupplierId` CHAR(36) NOT NULL COLLATE ascii_general_ci,
    `Name` VARCHAR(200) NOT NULL,
    `Code` VARCHAR(50) NOT NULL,
    `ContactPerson` VARCHAR(200) NULL,
    `Mobile` VARCHAR(20) NULL,
    `Email` VARCHAR(256) NULL,
    `Address` VARCHAR(500) NULL,
    `GstVatNumber` VARCHAR(15) NULL,
    `CreditPeriodDays` INT NULL,
    `CreditLimit` DECIMAL(18,2) NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NULL,
    `RowVersion` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`SupplierId`),
    UNIQUE INDEX `UX_Suppliers_Code` (`Code`),
    INDEX `IX_Suppliers_Name` (`Name`),
    INDEX `IX_Suppliers_Mobile` (`Mobile`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
