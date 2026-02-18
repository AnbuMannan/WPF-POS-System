namespace POS.Shared.Models;

public class SaleReturnDto
{
    public int ReturnId { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public string ReturnType { get; set; } = string.Empty;
    public decimal TotalReturnAmount { get; set; }
    public decimal RefundAmount { get; set; }
    public string? Reason { get; set; }
    public long OriginalSaleId { get; set; }
    public string? OriginalBillNumber { get; set; }
    public string? CustomerName { get; set; }
    public Guid? CustomerId { get; set; }
    public DateTime ReturnDate { get; set; }
    public string RefundMode { get; set; } = "Cash";
    public string Status { get; set; } = "Draft";
    public bool IsProcessed { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<SaleReturnItemDto> Items { get; set; } = new();
}

public class SaleReturnItemDto
{
    public int ReturnItemId { get; set; }
    public long SaleItemId { get; set; }
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? SKU { get; set; }
    public decimal QuantityReturned { get; set; }
    public decimal RefundPrice { get; set; }
    public decimal ReturnAmount { get; set; }
    public bool IsRestockable { get; set; } = true;
    public string? Reason { get; set; }
    public decimal OriginalQuantity { get; set; }
    public decimal AlreadyReturned { get; set; }
    public decimal MaxReturnQuantity { get; set; }
}

public class CreateSaleReturnDto
{
    public long OriginalSaleId { get; set; }
    public string? Reason { get; set; }
    public string RefundMode { get; set; } = "Cash";
    public List<CreateSaleReturnItemDto> Items { get; set; } = new();
}

public class CreateSaleReturnItemDto
{
    public long SaleItemId { get; set; }
    public long ProductId { get; set; }
    public decimal QuantityReturned { get; set; }
    public decimal RefundPrice { get; set; }
    public bool IsRestockable { get; set; } = true;
    public string? Reason { get; set; }
}

/// <summary>
/// DTO for looking up an invoice for return
/// </summary>
public class SaleInvoiceForReturnDto
{
    public long SaleId { get; set; }
    public string BillNumber { get; set; } = string.Empty;
    public string? InvoiceNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CustomerName { get; set; }
    public Guid? CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public List<SaleItemForReturnDto> Items { get; set; } = new();
}

public class SaleItemForReturnDto
{
    public long SaleItemId { get; set; }
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AlreadyReturned { get; set; }
    public decimal MaxReturnQuantity { get; set; }
    public bool IsReturned { get; set; }
}
