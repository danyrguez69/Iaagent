using IaAgent.Domain.Entities;
using IaAgent.Domain.Enums;

namespace IaAgent.Application.Ports;

public interface IListingRepository
{
    Task<List<MercadoLibreListing>> GetByStatusAsync(ListingStatus status, CancellationToken ct = default);
    Task AddAsync(MercadoLibreListing listing, CancellationToken ct = default);
    Task UpdateAsync(MercadoLibreListing listing, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
