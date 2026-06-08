using IaAgent.Domain.Entities;
using IaAgent.Domain.Enums;

namespace IaAgent.Application.Ports;

public interface IPurchaseOrderRepository
{
    Task<List<PurchaseOrder>> GetByStatusAsync(PurchaseStatus status, CancellationToken ct = default);
    Task<PurchaseOrder?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(PurchaseOrder order, CancellationToken ct = default);
    Task UpdateAsync(PurchaseOrder order, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
