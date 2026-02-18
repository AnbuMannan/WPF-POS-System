-- Create StockLedgerEntries table for inventory audit trail
CREATE TABLE IF NOT EXISTS StockLedgerEntries (
    StockEntryId CHAR(36) PRIMARY KEY COLLATE ascii_general_ci,
    ProductId BIGINT NOT NULL,
    Quantity DECIMAL(12, 3) NOT NULL,
    EntryType VARCHAR(20) NOT NULL, -- IN, OUT, ADJUSTMENT, RETURN
    ReferenceType VARCHAR(50) NOT NULL, -- PURCHASE_ENTRY, SALE, RETURN, ADJUSTMENT
    ReferenceId CHAR(36) NULL COLLATE ascii_general_ci,
    EntryDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Remarks VARCHAR(500) NULL,
    CONSTRAINT FK_StockLedgerEntries_Products_ProductId FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE RESTRICT
);

-- Indices for performance
CREATE INDEX IX_StockLedgerEntries_ProductId ON StockLedgerEntries(ProductId);
CREATE INDEX IX_StockLedgerEntries_ReferenceId ON StockLedgerEntries(ReferenceId);
CREATE INDEX IX_StockLedgerEntries_EntryDate ON StockLedgerEntries(EntryDate);
