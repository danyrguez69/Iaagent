using System.ComponentModel;
using System.Text.Json;
using IaAgent.Application.Ports;
using IaAgent.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace IaAgent.Application.Plugins;

public sealed class ChinaSearchPlugin
{
    private readonly IChinaMarketPort _chinaPort;
    private readonly IChineseProductRepository _repo;
    private readonly ILogger<ChinaSearchPlugin> _logger;

    public ChinaSearchPlugin(
        IChinaMarketPort chinaPort,
        IChineseProductRepository repo,
        ILogger<ChinaSearchPlugin> logger)
    {
        _chinaPort = chinaPort;
        _repo = repo;
        _logger = logger;
    }

    [KernelFunction("search_aliexpress_products")]
    [Description("Busca productos en AliExpress con palabras clave en inglés. Retorna JSON con lista de productos chinos.")]
    public async Task<string> SearchAliExpressProductsAsync(
        [Description("Palabras clave de búsqueda en inglés")] string keywords,
        [Description("Precio máximo en USD")] decimal maxPriceUsd,
        [Description("Rating mínimo del proveedor (0-5)")] decimal minRating = 4.5m)
    {
        try
        {
            var products = await _chinaPort.SearchProductsAsync(keywords, maxPriceUsd, minRating);
            _logger.LogInformation("Encontrados {Count} productos en AliExpress para '{Keywords}'",
                products.Count, keywords);
            return JsonSerializer.Serialize(products, new JsonSerializerOptions { WriteIndented = false });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error buscando en AliExpress: {Keywords}", keywords);
            return JsonSerializer.Serialize(new List<ChineseProduct>());
        }
    }

    [KernelFunction("get_shipping_cost_to_chile")]
    [Description("Obtiene el costo de envío estimado a Chile para un producto AliExpress. Retorna JSON con costo en USD y días estimados.")]
    public async Task<string> GetShippingCostToChileAsync(
        [Description("ID del producto en AliExpress")] string productId,
        [Description("Cantidad a ordenar")] int quantity = 1)
    {
        try
        {
            var cost = await _chinaPort.GetShippingCostAsync(productId, quantity, "CL");
            return JsonSerializer.Serialize(new { productId, shippingCostUsd = cost, destinationCountry = "CL" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo costo de envío para {ProductId}", productId);
            return JsonSerializer.Serialize(new { productId, shippingCostUsd = 15.0m, destinationCountry = "CL", estimated = true });
        }
    }

    [KernelFunction("rank_suppliers_by_reliability")]
    [Description("Ordena una lista de productos chinos por confiabilidad del proveedor. Retorna JSON con lista ordenada.")]
    public string RankSuppliersByReliability(
        [Description("JSON array de ChineseProduct")] string productsJson)
    {
        try
        {
            var products = JsonSerializer.Deserialize<List<ChineseProduct>>(productsJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

            var ranked = products
                .Where(p => p.SupplierRating >= 4.5m && p.EstimatedShippingDays <= 30)
                .OrderByDescending(p => p.SupplierRating * 0.4m + Math.Min(p.OrderCount / 1000m, 1m) * 0.6m)
                .ToList();

            return JsonSerializer.Serialize(ranked, new JsonSerializerOptions { WriteIndented = false });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ordenando proveedores");
            return productsJson;
        }
    }

    [KernelFunction("save_chinese_products")]
    [Description("Persiste los productos chinos encontrados vinculados a una oportunidad. Retorna cantidad guardada.")]
    public async Task<string> SaveChineseProductsAsync(
        [Description("ID de la oportunidad (GUID)")] string opportunityId,
        [Description("JSON array de ChineseProduct")] string productsJson)
    {
        try
        {
            var products = JsonSerializer.Deserialize<List<ChineseProduct>>(productsJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

            var oppId = Guid.Parse(opportunityId);
            foreach (var p in products)
            {
                p.Id = Guid.NewGuid();
                p.OpportunityId = oppId;
            }

            await _repo.AddRangeAsync(products);
            await _repo.SaveChangesAsync();

            return JsonSerializer.Serialize(new { success = true, savedCount = products.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error guardando productos chinos");
            return JsonSerializer.Serialize(new { success = false, error = ex.Message });
        }
    }
}
