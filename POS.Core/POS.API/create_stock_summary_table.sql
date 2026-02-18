-- Create StockSummary table for inventory management
CREATE TABLE IF NOT EXISTS StockSummary (
    ProductId BIGINT PRIMARY KEY,
    AvailableStock DECIMAL(18, 3) NOT NULL DEFAULT 0.000,
    LastUpdated DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT FK_StockSummary_Products_ProductId FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE CASCADE
);

-- Index for performance (though ProductId is PK)
CREATE INDEX IX_StockSummary_LastUpdated ON StockSummary(LastUpdated);
