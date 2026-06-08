using IaAgent.Application.Ports;
using IaAgent.Domain.Entities;
using IaAgent.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace IaAgent.Infrastructure.Persistence.Repositories;

public sealed class OpportunityRepository : IOpportunityRepository
{
    private readonly ArbitrageDbContext _db;

    public OpportunityRepository(ArbitrageDbContext db) => _db = db;

    public async Task<MarketOpportunity?> GetByMlProductIdAsync(string mlProductId, CancellationToken ct = default) =>
        await _db.Opportunities.FirstOrDefaultAsync(o => o.MercadoLibreProductId == mlProductId, ct);

    public async Task<List<MarketOpportunity>> GetByStatusAsync(OpportunityStatus status, CancellationToken ct = default) =>
        await _db.Opportunities
            .Include(o => o.ChineseProducts)
            .Include(o => o.Calculation)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.DetectedAt)
            .ToListAsync(ct);

    public async Task<List<MarketOpportunity>> GetTopByDemandScoreAsync(int count, CancellationToken ct = default) =>
        await _db.Opportunities
            .Include(o => o.ChineseProducts)
            .Include(o => o.Calculation)
            .Where(o => o.Status == OpportunityStatus.Detected || o.Status == OpportunityStatus.AnalyzingChina)
            .OrderByDescending(o => o.DemandScore)
            .Take(count)
            .ToListAsync(ct);

    public async Task AddAsync(MarketOpportunity opportunity, CancellationToken ct = default) =>
        await _db.Opportunities.AddAsync(opportunity, ct);

    public Task UpdateAsync(MarketOpportunity opportunity, CancellationToken ct = default)
    {
        _db.Opportunities.Update(opportunity);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default) =>
        await _db.SaveChangesAsync(ct);
}
