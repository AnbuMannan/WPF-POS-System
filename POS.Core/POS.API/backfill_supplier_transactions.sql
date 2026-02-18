-- SQL Script: Backfill SupplierTransactions for processed PurchaseEntries
-- Database: POS (MySQL)
-- Description: Creates missing supplier transactions for purchase entries that were 
--              processed before the transaction recording feature was added
-- 
-- IMPORTANT: Run this script ONCE after ensuring the SupplierTransactions table exists

-- =============================================
-- Step 1: Check for processed entries without transactions
-- =============================================
SELECT 
    pe.PurchaseEntryId,
    pe.SupplierId,
    pe.InvoiceNo,
    pe.TotalAmount,
    pe.ReceivedDate,
    pe.IsProcessed,
    s.Name AS SupplierName,
    (SELECT COUNT(*) FROM SupplierTransactions st 
     WHERE st.ReferenceId = pe.PurchaseEntryId AND st.TransactionType = 'Purchase') AS ExistingTxCount
FROM PurchaseEntries pe
JOIN Suppliers s ON pe.SupplierId = s.SupplierId
WHERE pe.IsProcessed = 1 
  AND pe.IsActive = 1
  AND NOT EXISTS (
      SELECT 1 FROM SupplierTransactions st 
      WHERE st.ReferenceId = pe.PurchaseEntryId 
        AND st.TransactionType = 'Purchase'
  );

-- =============================================
-- Step 2: Insert missing transactions
-- Run this after verifying the SELECT above returns the expected records
-- =============================================
INSERT INTO SupplierTransactions (
    SupplierTransactionId,
    SupplierId,
    TransactionDate,
    TransactionType,
    ReferenceId,
    ReferenceNo,
    DebitAmount,
    CreditAmount,
    Balance,
    Description,
    IsActive,
    CreatedAt
)
SELECT 
    UUID() AS SupplierTransactionId,
    pe.SupplierId,
    pe.ReceivedDate AS TransactionDate,
    'Purchase' AS TransactionType,
    pe.PurchaseEntryId AS ReferenceId,
    pe.InvoiceNo AS ReferenceNo,
    0.00 AS DebitAmount,
    pe.TotalAmount AS CreditAmount,
    pe.TotalAmount AS Balance,  -- Will be recalculated below
    CONCAT('Purchase Entry: ', pe.InvoiceNo) AS Description,
    1 AS IsActive,
    NOW() AS CreatedAt
FROM PurchaseEntries pe
WHERE pe.IsProcessed = 1 
  AND pe.IsActive = 1
  AND NOT EXISTS (
      SELECT 1 FROM SupplierTransactions st 
      WHERE st.ReferenceId = pe.PurchaseEntryId 
        AND st.TransactionType = 'Purchase'
  )
ORDER BY pe.ReceivedDate ASC;

-- =============================================
-- Step 3: Recalculate running balances
-- This updates the Balance column to be cumulative per supplier
-- =============================================
-- Create a temporary table to hold calculated balances
SET @running_balance := 0;
SET @current_supplier := '';

UPDATE SupplierTransactions st
JOIN (
    SELECT 
        SupplierTransactionId,
        SupplierId,
        TransactionDate,
        CreditAmount,
        DebitAmount,
        @running_balance := IF(@current_supplier = SupplierId, 
            @running_balance + CreditAmount - DebitAmount,
            CreditAmount - DebitAmount) AS new_balance,
        @current_supplier := SupplierId AS supplier_tracker
    FROM SupplierTransactions
    ORDER BY SupplierId, TransactionDate, CreatedAt
) calc ON st.SupplierTransactionId = calc.SupplierTransactionId
SET st.Balance = calc.new_balance;

-- =============================================
-- Step 4: Verify the results
-- =============================================
SELECT 
    s.Name AS SupplierName,
    s.Code AS SupplierCode,
    COUNT(st.SupplierTransactionId) AS TransactionCount,
    SUM(st.CreditAmount) AS TotalPurchases,
    SUM(st.DebitAmount) AS TotalDebits,
    (SELECT Balance FROM SupplierTransactions 
     WHERE SupplierId = s.SupplierId 
     ORDER BY TransactionDate DESC, CreatedAt DESC LIMIT 1) AS CurrentBalance
FROM Suppliers s
LEFT JOIN SupplierTransactions st ON s.SupplierId = st.SupplierId
WHERE s.IsActive = 1
GROUP BY s.SupplierId, s.Name, s.Code
HAVING TransactionCount > 0;
