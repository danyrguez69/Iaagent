using IaAgent.Application.Ports;
using IaAgent.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IaAgent.Infrastructure.Persistence.Repositories;

public sealed class ChineseProductRepository : IChineseProductRepository
{
    private readonly ArbitrageDbContext _db;

    public ChineseProductRepository(ArbitrageDbContext db) => _db = db;

    public async Task<List<ChineseProduct>> GetByOpportunityIdAsync(Guid opportunityId, CancellationToken ct = default) =>
        await _db.ChineseProducts
            .Where(p => p.OpportunityId == opportunityId)
            .OrderByDescending(p => p.SupplierRating)
            .ToListAsync(ct);

    public async Task AddRangeAsync(List<ChineseProduct> products, CancellationToken ct = default) =>
        await _db.ChineseProducts.AddRangeAsync(products, ct);

    public async Task SaveChangesAsync(CancellationToken ct = default) =>
        await _db.SaveChangesAsync(ct);
}
