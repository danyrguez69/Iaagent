using IaAgent.Application.Ports;
using IaAgent.Domain.Entities;
using IaAgent.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace IaAgent.Infrastructure.Persistence.Repositories;

public sealed class ListingRepository : IListingRepository
{
    private readonly ArbitrageDbContext _db;

    public ListingRepository(ArbitrageDbContext db) => _db = db;

    public async Task<List<MercadoLibreListing>> GetByStatusAsync(ListingStatus status, CancellationToken ct = default) =>
        await _db.Listings
            .Where(l => l.Status == status)
            .OrderByDescending(l => l.PublishedAt)
            .ToListAsync(ct);

    public async Task AddAsync(MercadoLibreListing listing, CancellationToken ct = default) =>
        await _db.Listings.AddAsync(listing, ct);

    public Task UpdateAsync(MercadoLibreListing listing, CancellationToken ct = default)
    {
        _db.Listings.Update(listing);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default) =>
        await _db.SaveChangesAsync(ct);
}
