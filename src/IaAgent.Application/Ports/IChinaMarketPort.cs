using IaAgent.Domain.Entities;

namespace IaAgent.Application.Ports;

public interface IChinaMarketPort
{
    Task<List<ChineseProduct>> SearchProductsAsync(
        string keywords,
        decimal maxPriceUsd,
        decimal minRating = 4.5m,
        int pageSize = 20,
        CancellationToken ct = default);

    Task<decimal> GetShippingCostAsync(
        string productId,
        int quantity,
        string destinationCountry = "CL",
        CancellationToken ct = default);

    Task<ChineseProduct?> GetProductDetailAsync(string productId, CancellationToken ct = default);
}
