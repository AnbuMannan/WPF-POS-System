-- ============================================================
-- Cash Transactions & Company Profile Tables
-- Run this script on your MySQL pos database
-- ============================================================

-- ============================================================
-- Cash Transactions Table
-- ============================================================
CREATE TABLE IF NOT EXISTS CashTransactions (
    CashTransactionId CHAR(36) PRIMARY KEY,
    TransactionDate DATETIME NOT NULL,
    Type VARCHAR(20) NOT NULL COMMENT 'CashIn or CashOut',
    Amount DECIMAL(18,2) NOT NULL,
    Description VARCHAR(500) NULL,
    ReferenceNo VARCHAR(50) NULL,
    Category VARCHAR(50) NULL,
    UserId INT NOT NULL,
    UserName VARCHAR(100) NULL,
    Remarks VARCHAR(500) NULL,
    IsActive TINYINT(1) DEFAULT 1,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    RowVersion TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX IX_CashTransactions_TransactionDate (TransactionDate),
    INDEX IX_CashTransactions_Type (Type),
    INDEX IX_CashTransactions_UserId (UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ============================================================
-- Company Profile Table (Single record)
-- ============================================================
CREATE TABLE IF NOT EXISTS CompanyProfiles (
    CompanyProfileId INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(200) NOT NULL,
    Address VARCHAR(500) NULL,
    City VARCHAR(100) NULL,
    State VARCHAR(100) NULL,
    PostalCode VARCHAR(20) NULL,
    Country VARCHAR(100) NULL,
    Phone VARCHAR(20) NULL,
    Mobile VARCHAR(20) NULL,
    Email VARCHAR(100) NULL,
    Website VARCHAR(200) NULL,
    GstNumber VARCHAR(50) NULL,
    PanNumber VARCHAR(50) NULL,
    LogoUrl VARCHAR(500) NULL,
    CurrencySymbol VARCHAR(10) DEFAULT '₹',
    CurrencyCode VARCHAR(10) DEFAULT 'INR',
    ReceiptHeader VARCHAR(100) NULL,
    ReceiptFooter VARCHAR(200) NULL,
    IsActive TINYINT(1) DEFAULT 1,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ============================================================
-- Insert Default Company Profile
-- ============================================================
INSERT INTO CompanyProfiles (Name, CurrencySymbol, CurrencyCode, CreatedAt)
SELECT 'My Company', '₹', 'INR', NOW()
WHERE NOT EXISTS (SELECT 1 FROM CompanyProfiles);
