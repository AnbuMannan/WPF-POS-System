-- SystemPreferences Table Schema
-- Stores system-wide preferences and UI settings for each store
CREATE TABLE SystemPreferences (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    StoreCode INT NOT NULL,
    SidebarIdleTimeoutSeconds INT NOT NULL DEFAULT 10,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Index for store-based queries
    INDEX IX_SystemPreferences_StoreCode (StoreCode),
    
    -- Unique constraint to ensure one preference record per store
    UNIQUE KEY UK_SystemPreferences_StoreCode (StoreCode)
    
    -- Foreign key constraint (if Stores table exists) - uncomment if needed
    -- CONSTRAINT FK_SystemPreferences_Stores_StoreCode FOREIGN KEY (StoreCode) REFERENCES Stores(StoreCode)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Seed data for initial stores
INSERT INTO SystemPreferences (Id, StoreCode, SidebarIdleTimeoutSeconds, CreatedAt, UpdatedAt) VALUES
(1, 1, 10, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(2, 2, 10, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);

-- Optional: Add comments for documentation
ALTER TABLE SystemPreferences COMMENT = 'Stores system preferences and UI settings for each store';

-- Column comments
ALTER TABLE SystemPreferences 
    MODIFY COLUMN Id INT COMMENT 'Primary key identifier',
    MODIFY COLUMN StoreCode INT COMMENT 'Store code for which these preferences apply (references Stores.StoreCode)',
    MODIFY COLUMN SidebarIdleTimeoutSeconds INT COMMENT 'Sidebar idle timeout in seconds (default: 10)',
    MODIFY COLUMN CreatedAt DATETIME COMMENT 'Record creation timestamp',
    MODIFY COLUMN UpdatedAt DATETIME COMMENT 'Record last update timestamp';

-- Show table structure
DESCRIBE SystemPreferences;