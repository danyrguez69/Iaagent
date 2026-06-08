using System.ComponentModel;
using System.Text.Json;
using IaAgent.Application.Ports;
using IaAgent.Domain.Constants;
using IaAgent.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace IaAgent.Application.Plugins;

public sealed class MarketAnalysisPlugin
{
    private readonly IMercadoLibrePort _mlPort;
    private readonly IOpportunityRepository _repo;
    private readonly ILogger<MarketAnalysisPlugin> _logger;

    public MarketAnalysisPlugin(
        IMercadoLibrePort mlPort,
        IOpportunityRepository repo,
        ILogger<MarketAnalysisPlugin> logger)
    {
        _mlPort = mlPort;
        _repo = repo;
        _logger = logger;
    }

    [KernelFunction("search_trending_products")]
    [Description("Busca productos trending en MercadoLibre Chile para una categoría dada. Retorna JSON con lista de oportunidades.")]
    public async Task<string> SearchTrendingProductsAsync(
        [Description("ID de categoría ML, ej: MLC1000")] string categoryId,
        [Description("Cantidad máxima de resultados")] int limit = 20)
    {
        try
        {
            var products = await _mlPort.GetTrendingProductsAsync(categoryId, limit);
            _logger.LogInformation("Encontrados {Count} productos trending en {Category}", products.Count, categoryId);
            return JsonSerializer.Serialize(products, new JsonSerializerOptions { WriteIndented = false });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error buscando productos trending en {Category}", categoryId);
            return JsonSerializer.Serialize(new List<MarketOpportunity>());
        }
    }

    [KernelFunction("search_products_by_query")]
    [Description("Busca productos en MercadoLibre Chile por texto. Retorna JSON con lista de productos.")]
    public async Task<string> SearchProductsByQueryAsync(
        [Description("Texto de búsqueda en español")] string query,
        [Description("Cantidad máxima de resultados")] int limit = 30)
    {
        try
        {
            var products = await _mlPort.SearchProductsAsync(query, limit);
            return JsonSerializer.Serialize(products, new JsonSerializerOptions { WriteIndented = false });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error buscando productos con query: {Query}", query);
            return JsonSerializer.Serialize(new List<MarketOpportunity>());
        }
    }

    [KernelFunction("get_demand_score")]
    [Description("Calcula el score de demanda (0-100) de un producto ML basado en ventas y visitas.")]
    public async Task<string> GetDemandScoreAsync(
        [Description("ID del item en MercadoLibre")] string itemId)
    {
        try
        {
            var score = await _mlPort.GetProductDemandScoreAsync(itemId);
            return JsonSerializer.Serialize(new { itemId, demandScore = score });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculando demand score para {ItemId}", itemId);
            return JsonSerializer.Serialize(new { itemId, demandScore = 0 });
        }
    }

    [KernelFunction("save_opportunity")]
    [Description("Persiste una oportunidad de mercado detectada en la base de datos. Retorna el ID asignado.")]
    public async Task<string> SaveOpportunityAsync(
        [Description("JSON serializado de MarketOpportunity")] string opportunityJson)
    {
        try
        {
            var opportunity = JsonSerializer.Deserialize<MarketOpportunity>(opportunityJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (opportunity is null)
                return JsonSerializer.Serialize(new { success = false, error = "JSON inválido" });

            var existing = await _repo.GetByMlProductIdAsync(opportunity.MercadoLibreProductId);
            if (existing is not null)
                return JsonSerializer.Serialize(new { success = true, id = existing.Id, message = "Ya existe" });

            opportunity.Id = Guid.NewGuid();
            await _repo.AddAsync(opportunity);
            await _repo.SaveChangesAsync();

            _logger.LogInformation("Oportunidad guardada: {Title} (score: {Score})",
                opportunity.ProductTitle, opportunity.DemandScore);

            return JsonSerializer.Serialize(new { success = true, id = opportunity.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error guardando oportunidad");
            return JsonSerializer.Serialize(new { success = false, error = ex.Message });
        }
    }

    [KernelFunction("get_site_id")]
    [Description("Retorna el Site ID de MercadoLibre para Chile.")]
    public string GetChileSiteId() => ChileImportConstants.MercadoLibreChileSiteId;
}
