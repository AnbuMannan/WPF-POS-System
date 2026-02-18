-- ============================================================
-- Users, Roles & Permissions Schema for pos_auth database
-- Run this script on your MySQL pos_auth database
-- Compatible with MySQL 5.7+ and MySQL 8.x
-- ============================================================

-- ============================================================
-- Add missing columns to Users table (MySQL compatible)
-- Uses prepared statements without DELIMITER
-- ============================================================

-- Add Email column to Users table if not exists
SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Users' AND COLUMN_NAME = 'Email') = 0,
    'ALTER TABLE Users ADD COLUMN Email VARCHAR(256) NULL',
    'SELECT 1'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Add FullName column to Users table if not exists
SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Users' AND COLUMN_NAME = 'FullName') = 0,
    'ALTER TABLE Users ADD COLUMN FullName VARCHAR(200) NULL',
    'SELECT 1'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Add Phone column to Users table if not exists
SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Users' AND COLUMN_NAME = 'Phone') = 0,
    'ALTER TABLE Users ADD COLUMN Phone VARCHAR(20) NULL',
    'SELECT 1'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Add CreatedAt column to Users table if not exists
SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Users' AND COLUMN_NAME = 'CreatedAt') = 0,
    'ALTER TABLE Users ADD COLUMN CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP',
    'SELECT 1'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Add UpdatedAt column to Users table if not exists
SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Users' AND COLUMN_NAME = 'UpdatedAt') = 0,
    'ALTER TABLE Users ADD COLUMN UpdatedAt DATETIME NULL',
    'SELECT 1'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ============================================================
-- Add missing columns to Roles table
-- ============================================================

-- Add Description column to Roles table if not exists
SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Roles' AND COLUMN_NAME = 'Description') = 0,
    'ALTER TABLE Roles ADD COLUMN Description VARCHAR(255) NULL',
    'SELECT 1'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Add IsActive column to Roles table if not exists
SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Roles' AND COLUMN_NAME = 'IsActive') = 0,
    'ALTER TABLE Roles ADD COLUMN IsActive TINYINT(1) DEFAULT 1',
    'SELECT 1'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Add CreatedAt column to Roles table if not exists
SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Roles' AND COLUMN_NAME = 'CreatedAt') = 0,
    'ALTER TABLE Roles ADD COLUMN CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP',
    'SELECT 1'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Add UpdatedAt column to Roles table if not exists
SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Roles' AND COLUMN_NAME = 'UpdatedAt') = 0,
    'ALTER TABLE Roles ADD COLUMN UpdatedAt DATETIME NULL',
    'SELECT 1'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ============================================================
-- Permissions Table
-- ============================================================
CREATE TABLE IF NOT EXISTS Permissions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Code VARCHAR(100) NOT NULL UNIQUE,
    Description VARCHAR(255) NOT NULL,
    Module VARCHAR(50) NOT NULL,
    IsActive TINYINT(1) DEFAULT 1,
    INDEX idx_permissions_module (Module),
    INDEX idx_permissions_code (Code)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ============================================================
-- RolePermissions Junction Table
-- ============================================================
CREATE TABLE IF NOT EXISTS RolePermissions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    RoleId INT NOT NULL,
    PermissionId INT NOT NULL,
    FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE,
    FOREIGN KEY (PermissionId) REFERENCES Permissions(Id) ON DELETE CASCADE,
    UNIQUE KEY unique_role_permission (RoleId, PermissionId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ============================================================
-- Seed Default Roles
-- ============================================================
INSERT IGNORE INTO Roles (Id, Name, Description, IsActive, CreatedAt) VALUES
(1, 'Admin', 'Full system access', 1, NOW()),
(2, 'Manager', 'Management level access', 1, NOW()),
(3, 'Cashier', 'Point of Sale access only', 1, NOW()),
(4, 'Inventory', 'Inventory management access', 1, NOW());

-- ============================================================
-- Seed Default Permissions
-- ============================================================
INSERT IGNORE INTO Permissions (Code, Description, Module) VALUES
-- Sales Module
('Sales.Create', 'Create new sales', 'Sales'),
('Sales.View', 'View sales history', 'Sales'),
('Sales.Void', 'Void/Cancel sales', 'Sales'),
('Sales.Discount', 'Apply discounts', 'Sales'),
('Sales.Return', 'Process returns', 'Sales'),
('Sales.Reprint', 'Reprint receipts', 'Sales'),

-- Products Module
('Products.View', 'View products', 'Products'),
('Products.Create', 'Create products', 'Products'),
('Products.Edit', 'Edit products', 'Products'),
('Products.Delete', 'Delete products', 'Products'),
('Products.PriceChange', 'Change product prices', 'Products'),

-- Inventory Module
('Inventory.View', 'View inventory', 'Inventory'),
('Inventory.Adjust', 'Adjust stock', 'Inventory'),
('Inventory.Transfer', 'Transfer stock', 'Inventory'),
('Inventory.ItemLedger', 'View item ledger', 'Inventory'),
('Inventory.PrintLabels', 'Print product labels', 'Inventory'),

-- Purchase Module
('Purchase.View', 'View purchase orders', 'Purchase'),
('Purchase.Create', 'Create purchase orders', 'Purchase'),
('Purchase.Approve', 'Approve purchase orders', 'Purchase'),
('Purchase.Receive', 'Receive goods (GRN)', 'Purchase'),
('Purchase.Return', 'Return to supplier', 'Purchase'),

-- Suppliers Module
('Suppliers.View', 'View suppliers', 'Suppliers'),
('Suppliers.Create', 'Create suppliers', 'Suppliers'),
('Suppliers.Edit', 'Edit suppliers', 'Suppliers'),
('Suppliers.Delete', 'Delete suppliers', 'Suppliers'),
('Suppliers.Payments', 'Manage supplier payments', 'Suppliers'),

-- Customers Module
('Customers.View', 'View customers', 'Customers'),
('Customers.Create', 'Create customers', 'Customers'),
('Customers.Edit', 'Edit customers', 'Customers'),
('Customers.Delete', 'Delete customers', 'Customers'),

-- Reports Module
('Reports.View', 'View reports', 'Reports'),
('Reports.Sales', 'View sales reports', 'Reports'),
('Reports.Inventory', 'View inventory reports', 'Reports'),
('Reports.Financial', 'View financial reports', 'Reports'),
('Reports.Export', 'Export reports', 'Reports'),

-- Cash Management Module
('Cash.View', 'View cash transactions', 'Cash'),
('Cash.CashIn', 'Add cash to drawer', 'Cash'),
('Cash.CashOut', 'Remove cash from drawer', 'Cash'),
('Cash.DayEnd', 'Perform day-end', 'Cash'),

-- User Management Module
('Users.View', 'View users', 'Users'),
('Users.Create', 'Create users', 'Users'),
('Users.Edit', 'Edit users', 'Users'),
('Users.Delete', 'Delete users', 'Users'),
('Users.ResetPassword', 'Reset user passwords', 'Users'),

-- Roles & Permissions Module
('Roles.View', 'View roles', 'Roles'),
('Roles.Create', 'Create roles', 'Roles'),
('Roles.Edit', 'Edit roles', 'Roles'),
('Roles.Permissions', 'Manage role permissions', 'Roles'),

-- Settings Module
('Settings.View', 'View settings', 'Settings'),
('Settings.Edit', 'Edit settings', 'Settings'),
('Settings.Company', 'Manage company profile', 'Settings'),
('Settings.Print', 'Configure print settings', 'Settings');

-- ============================================================
-- Assign All Permissions to Admin Role
-- ============================================================
INSERT IGNORE INTO RolePermissions (RoleId, PermissionId)
SELECT 1, Id FROM Permissions;

-- ============================================================
-- Assign Cashier Permissions
-- ============================================================
INSERT IGNORE INTO RolePermissions (RoleId, PermissionId)
SELECT 3, Id FROM Permissions WHERE Code IN (
    'Sales.Create', 'Sales.View', 'Sales.Discount', 'Sales.Return', 'Sales.Reprint',
    'Products.View', 'Customers.View', 'Customers.Create',
    'Cash.View', 'Cash.CashIn', 'Cash.CashOut'
);

-- ============================================================
-- Assign Manager Permissions
-- ============================================================
INSERT IGNORE INTO RolePermissions (RoleId, PermissionId)
SELECT 2, Id FROM Permissions WHERE Code IN (
    'Sales.Create', 'Sales.View', 'Sales.Void', 'Sales.Discount', 'Sales.Return', 'Sales.Reprint',
    'Products.View', 'Products.Create', 'Products.Edit', 'Products.PriceChange',
    'Inventory.View', 'Inventory.Adjust', 'Inventory.ItemLedger',
    'Purchase.View', 'Purchase.Create', 'Purchase.Receive',
    'Suppliers.View', 'Suppliers.Create', 'Suppliers.Edit', 'Suppliers.Payments',
    'Customers.View', 'Customers.Create', 'Customers.Edit',
    'Reports.View', 'Reports.Sales', 'Reports.Inventory',
    'Cash.View', 'Cash.CashIn', 'Cash.CashOut', 'Cash.DayEnd',
    'Users.View'
);

-- ============================================================
-- Assign Inventory Role Permissions
-- ============================================================
INSERT IGNORE INTO RolePermissions (RoleId, PermissionId)
SELECT 4, Id FROM Permissions WHERE Code IN (
    'Products.View', 'Products.Create', 'Products.Edit',
    'Inventory.View', 'Inventory.Adjust', 'Inventory.Transfer', 'Inventory.ItemLedger', 'Inventory.PrintLabels',
    'Purchase.View', 'Purchase.Create', 'Purchase.Receive', 'Purchase.Return',
    'Suppliers.View', 'Suppliers.Create', 'Suppliers.Edit',
    'Reports.Inventory'
);
