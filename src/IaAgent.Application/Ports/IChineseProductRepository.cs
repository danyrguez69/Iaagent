using IaAgent.Domain.Entities;

namespace IaAgent.Application.Ports;

public interface IChineseProductRepository
{
    Task<List<ChineseProduct>> GetByOpportunityIdAsync(Guid opportunityId, CancellationToken ct = default);
    Task AddRangeAsync(List<ChineseProduct> products, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
