using System.ComponentModel;
using System.Text.Json;
using IaAgent.Application.Ports;
using IaAgent.Domain.DomainServices;
using IaAgent.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace IaAgent.Application.Plugins;

public sealed class ArbitragePlugin
{
    private readonly IExchangeRatePort _exchangePort;
    private readonly ILogger<ArbitragePlugin> _logger;

    public ArbitragePlugin(IExchangeRatePort exchangePort, ILogger<ArbitragePlugin> logger)
    {
        _exchangePort = exchangePort;
        _logger = logger;
    }

    [KernelFunction("get_usd_clp_rate")]
    [Description("Obtiene la tasa de cambio actual USD a CLP desde la API de tipo de cambio.")]
    public async Task<string> GetUsdClpRateAsync()
    {
        try
        {
            var rate = await _exchangePort.GetUsdToClpRateAsync();
            return JsonSerializer.Serialize(new { usdToClp = rate, source = "open.er-api.com" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo tipo de cambio");
            return JsonSerializer.Serialize(new { usdToClp = 950m, source = "fallback", error = ex.Message });
        }
    }

    [KernelFunction("calculate_arbitrage_margin")]
    [Description("Calcula el margen de arbitraje completo para un par (producto chino, precio de venta). Incluye arancel 6%, IVA 19%, comisión ML 12.99% y buffer 10%. Retorna ArbitrageCalculation en JSON.")]
    public async Task<string> CalculateArbitrageMarginAsync(
        [Description("JSON de ChineseProduct")] string chineseProductJson,
        [Description("JSON de MarketOpportunity")] string opportunityJson,
        [Description("Precio de venta objetivo en CLP")] decimal targetSellingPriceClp)
    {
        try
        {
            var product = JsonSerializer.Deserialize<ChineseProduct>(chineseProductJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var opportunity = JsonSerializer.Deserialize<MarketOpportunity>(opportunityJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (product is null || opportunity is null)
                return JsonSerializer.Serialize(new { error = "JSON inválido" });

            var rate = await _exchangePort.GetUsdToClpRateAsync();
            var calculation = ArbitrageDomainService.Calculate(product, opportunity, rate, targetSellingPriceClp);

            _logger.LogInformation(
                "Arbitraje calculado: margen {Margin:P1}, ROI {Roi:P1}, rentable: {Profitable}",
                calculation.NetMarginPct, calculation.Roi, calculation.IsProfitable);

            return JsonSerializer.Serialize(calculation, new JsonSerializerOptions { WriteIndented = false });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculando arbitraje");
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [KernelFunction("suggest_optimal_price")]
    [Description("Sugiere el precio de venta óptimo en CLP dado el costo total en USD para alcanzar el margen mínimo.")]
    public async Task<string> SuggestOptimalPriceAsync(
        [Description("Costo total en USD (producto + envío)")] decimal totalCostUsd,
        [Description("Precio promedio de la competencia en CLP (referencia)")] decimal competitorAvgPriceClp,
        [Description("Margen mínimo deseado")] decimal minMarginPct = 0.30m)
    {
        try
        {
            var rate = await _exchangePort.GetUsdToClpRateAsync();
            var minPrice = ArbitrageDomainService.SuggestSellingPrice(totalCostUsd, rate, minMarginPct);

            var suggestedPrice = Math.Min(minPrice, competitorAvgPriceClp * 0.95m);
            suggestedPrice = Math.Max(suggestedPrice, minPrice);

            return JsonSerializer.Serialize(new
            {
                suggestedPriceClp = suggestedPrice,
                minViablePriceClp = minPrice,
                competitorRefClp = competitorAvgPriceClp,
                usdToClp = rate
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sugiriendo precio");
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }
}
