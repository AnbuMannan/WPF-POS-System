using Microsoft.EntityFrameworkCore;
using POS.Application.Interfaces.Repositories;
using POS.Domain.Entities;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class SupplierPaymentRepository : ISupplierPaymentRepository
{
    private readonly PosDbContext _context;

    public SupplierPaymentRepository(PosDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SupplierPayment>> GetAllAsync(bool includeInactive = false)
    {
        var query = _context.SupplierPayments
            .Include(p => p.Supplier)
            .AsQueryable();

        if (!includeInactive)
            query = query.Where(p => p.IsActive);

        return await query
            .OrderByDescending(p => p.PaymentDate)
            .ThenByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<SupplierPayment?> GetByIdAsync(Guid id)
    {
        return await _context.SupplierPayments
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<SupplierPayment>> GetBySupplierAsync(Guid supplierId)
    {
        return await _context.SupplierPayments
            .Include(p => p.Supplier)
            .Where(p => p.SupplierId == supplierId && p.IsActive)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }

    public async Task<SupplierPayment> CreateAsync(SupplierPayment payment)
    {
        _context.SupplierPayments.Add(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<SupplierPayment> UpdateAsync(SupplierPayment payment)
    {
        _context.SupplierPayments.Update(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<bool> DisableAsync(Guid id)
    {
        var payment = await _context.SupplierPayments.FindAsync(id);
        if (payment == null) return false;

        payment.IsActive = false;
        payment.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string> GeneratePaymentNoAsync()
    {
        var today = DateTime.Now;
        var prefix = $"PAY{today:yyyyMMdd}";
        
        var lastPayment = await _context.SupplierPayments
            .Where(p => p.PaymentNo.StartsWith(prefix))
            .OrderByDescending(p => p.PaymentNo)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastPayment != null && lastPayment.PaymentNo.Length > prefix.Length)
        {
            var seqStr = lastPayment.PaymentNo.Substring(prefix.Length);
            if (int.TryParse(seqStr, out int lastSeq))
                sequence = lastSeq + 1;
        }

        return $"{prefix}{sequence:D4}";
    }

    public async Task<bool> PaymentNoExistsAsync(string paymentNo, Guid? excludeId = null)
    {
        var query = _context.SupplierPayments.Where(p => p.PaymentNo == paymentNo);
        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);
        return await query.AnyAsync();
    }
}
