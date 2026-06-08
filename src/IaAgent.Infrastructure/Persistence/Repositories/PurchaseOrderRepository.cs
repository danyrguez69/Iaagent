using IaAgent.Application.Ports;
using IaAgent.Domain.Entities;
using IaAgent.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace IaAgent.Infrastructure.Persistence.Repositories;

public sealed class PurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly ArbitrageDbContext _db;

    public PurchaseOrderRepository(ArbitrageDbContext db) => _db = db;

    public async Task<List<PurchaseOrder>> GetByStatusAsync(PurchaseStatus status, CancellationToken ct = default) =>
        await _db.PurchaseOrders
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);

    public async Task<PurchaseOrder?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.PurchaseOrders.FindAsync(new object[] { id }, ct);

    public async Task AddAsync(PurchaseOrder order, CancellationToken ct = default) =>
        await _db.PurchaseOrders.AddAsync(order, ct);

    public Task UpdateAsync(PurchaseOrder order, CancellationToken ct = default)
    {
        _db.PurchaseOrders.Update(order);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default) =>
        await _db.SaveChangesAsync(ct);
}
