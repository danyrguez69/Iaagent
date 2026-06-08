using IaAgent.Domain.Entities;

namespace IaAgent.Application.Ports;

public interface IMercadoLibrePort
{
    Task<List<MarketOpportunity>> GetTrendingProductsAsync(string categoryId, int limit = 20, CancellationToken ct = default);
    Task<List<MarketOpportunity>> SearchProductsAsync(string query, int limit = 50, CancellationToken ct = default);
    Task<decimal> GetProductDemandScoreAsync(string itemId, CancellationToken ct = default);
    Task<string> CreateListingAsync(MercadoLibreListing listing, CancellationToken ct = default);
    Task UpdateListingAsync(string listingId, MercadoLibreListing listing, CancellationToken ct = default);
    Task<List<string>> UploadImagesAsync(List<string> imageUrls, CancellationToken ct = default);
    Task<bool> RefreshTokenAsync(CancellationToken ct = default);
}
