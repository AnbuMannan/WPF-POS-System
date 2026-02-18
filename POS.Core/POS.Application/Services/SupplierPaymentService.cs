using POS.Application.Interfaces.Repositories;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;
using POS.Shared.Models;

namespace POS.Application.Services;

public class SupplierPaymentService : ISupplierPaymentService
{
    private readonly ISupplierPaymentRepository _paymentRepository;
    private readonly ISupplierTransactionService _transactionService;
    private readonly ISupplierRepository _supplierRepository;

    public SupplierPaymentService(
        ISupplierPaymentRepository paymentRepository,
        ISupplierTransactionService transactionService,
        ISupplierRepository supplierRepository)
    {
        _paymentRepository = paymentRepository;
        _transactionService = transactionService;
        _supplierRepository = supplierRepository;
    }

    public async Task<IEnumerable<SupplierPaymentDto>> GetAllAsync(bool includeInactive = false)
    {
        var payments = await _paymentRepository.GetAllAsync(includeInactive);
        return payments.Select(MapToDto);
    }

    public async Task<SupplierPaymentDto?> GetByIdAsync(Guid id)
    {
        var payment = await _paymentRepository.GetByIdAsync(id);
        return payment != null ? MapToDto(payment) : null;
    }

    public async Task<IEnumerable<SupplierPaymentDto>> GetBySupplierAsync(Guid supplierId)
    {
        var payments = await _paymentRepository.GetBySupplierAsync(supplierId);
        return payments.Select(MapToDto);
    }

    public async Task<SupplierPaymentDto> CreateAsync(CreateSupplierPaymentDto dto)
    {
        // Validate supplier exists
        var supplier = await _supplierRepository.GetByIdAsync(dto.SupplierId);
        if (supplier == null)
            throw new InvalidOperationException($"Supplier with ID {dto.SupplierId} not found.");

        // Validate amount
        if (dto.Amount <= 0)
            throw new InvalidOperationException("Payment amount must be greater than zero.");

        // Generate payment number
        var paymentNo = await _paymentRepository.GeneratePaymentNoAsync();

        var payment = new SupplierPayment
        {
            Id = Guid.NewGuid(),
            SupplierId = dto.SupplierId,
            PaymentDate = dto.PaymentDate,
            Amount = dto.Amount,
            PaymentMode = dto.PaymentMode,
            ReferenceNo = dto.ReferenceNo,
            BankName = dto.BankName,
            Remarks = dto.Remarks,
            PaymentNo = paymentNo,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        var created = await _paymentRepository.CreateAsync(payment);

        // Record transaction in supplier ledger
        await _transactionService.RecordPaymentAsync(
            dto.SupplierId,
            created.Id,
            paymentNo,
            dto.Amount,
            $"Payment via {dto.PaymentMode}" + (string.IsNullOrEmpty(dto.ReferenceNo) ? "" : $" - Ref: {dto.ReferenceNo}")
        );

        // Reload with navigation properties
        var result = await _paymentRepository.GetByIdAsync(created.Id);
        return MapToDto(result!);
    }

    public async Task<SupplierPaymentDto> UpdateAsync(Guid id, CreateSupplierPaymentDto dto)
    {
        var existing = await _paymentRepository.GetByIdAsync(id);
        if (existing == null)
            throw new InvalidOperationException($"Payment with ID {id} not found.");

        // Note: Updating payments after ledger entry is complex
        // For now, we only allow updating non-financial fields
        existing.PaymentDate = dto.PaymentDate;
        existing.PaymentMode = dto.PaymentMode;
        existing.ReferenceNo = dto.ReferenceNo;
        existing.BankName = dto.BankName;
        existing.Remarks = dto.Remarks;
        existing.UpdatedAt = DateTime.Now;

        var updated = await _paymentRepository.UpdateAsync(existing);
        return MapToDto(updated);
    }

    public async Task<bool> DisableAsync(Guid id)
    {
        // Note: Disabling a payment after ledger entry would require reversing the transaction
        // For simplicity, we just mark it inactive but the ledger entry remains
        return await _paymentRepository.DisableAsync(id);
    }

    public async Task<bool> PaymentNoExistsAsync(string paymentNo, Guid? excludeId = null)
    {
        return await _paymentRepository.PaymentNoExistsAsync(paymentNo, excludeId);
    }

    private static SupplierPaymentDto MapToDto(SupplierPayment payment)
    {
        return new SupplierPaymentDto
        {
            Id = payment.Id,
            SupplierId = payment.SupplierId,
            SupplierName = payment.Supplier?.Name,
            SupplierCode = payment.Supplier?.Code,
            PaymentDate = payment.PaymentDate,
            Amount = payment.Amount,
            PaymentMode = payment.PaymentMode,
            ReferenceNo = payment.ReferenceNo,
            BankName = payment.BankName,
            Remarks = payment.Remarks,
            PaymentNo = payment.PaymentNo,
            IsActive = payment.IsActive,
            CreatedAt = payment.CreatedAt
        };
    }
}
