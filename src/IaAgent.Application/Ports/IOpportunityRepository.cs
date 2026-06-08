using IaAgent.Domain.Entities;
using IaAgent.Domain.Enums;

namespace IaAgent.Application.Ports;

public interface IOpportunityRepository
{
    Task<MarketOpportunity?> GetByMlProductIdAsync(string mlProductId, CancellationToken ct = default);
    Task<List<MarketOpportunity>> GetByStatusAsync(OpportunityStatus status, CancellationToken ct = default);
    Task<List<MarketOpportunity>> GetTopByDemandScoreAsync(int count, CancellationToken ct = default);
    Task AddAsync(MarketOpportunity opportunity, CancellationToken ct = default);
    Task UpdateAsync(MarketOpportunity opportunity, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
