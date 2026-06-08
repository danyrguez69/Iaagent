using IaAgent.Application.Ports;
using IaAgent.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace IaAgent.Application.Agents;

public sealed class OrchestratorAgent
{
    private readonly MarketAnalysisAgent _marketAgent;
    private readonly ChinaSearchAgent _chinaAgent;
    private readonly ArbitrageCalculatorAgent _calculatorAgent;
    private readonly PurchaseAgent _purchaseAgent;
    private readonly MercadoLibrePublisherAgent _publisherAgent;
    private readonly IOpportunityRepository _opportunityRepo;
    private readonly IChineseProductRepository _chineseProductRepo;
    private readonly IPurchaseOrderRepository _purchaseOrderRepo;
    private readonly ILogger<OrchestratorAgent> _logger;

    public OrchestratorAgent(
        MarketAnalysisAgent marketAgent,
        ChinaSearchAgent chinaAgent,
        ArbitrageCalculatorAgent calculatorAgent,
        PurchaseAgent purchaseAgent,
        MercadoLibrePublisherAgent publisherAgent,
        IOpportunityRepository opportunityRepo,
        IChineseProductRepository chineseProductRepo,
        IPurchaseOrderRepository purchaseOrderRepo,
        ILogger<OrchestratorAgent> logger)
    {
        _marketAgent = marketAgent;
        _chinaAgent = chinaAgent;
        _calculatorAgent = calculatorAgent;
        _purchaseAgent = purchaseAgent;
        _publisherAgent = publisherAgent;
        _opportunityRepo = opportunityRepo;
        _chineseProductRepo = chineseProductRepo;
        _purchaseOrderRepo = purchaseOrderRepo;
        _logger = logger;
    }

    public async Task RunPipelineAsync(List<string> categoryIds, CancellationToken ct = default)
    {
        _logger.LogInformation("=== PIPELINE DE ARBITRAJE INICIADO === {Time}", DateTime.UtcNow);

        try
        {
            // Fase 1: Análisis de mercado chileno
            _logger.LogInformation("[FASE 1] Analizando mercado MercadoLibre Chile...");
            await _marketAgent.AnalyzeMarketAsync(categoryIds, ct);

            var opportunities = await _opportunityRepo.GetTopByDemandScoreAsync(10, ct);
            _logger.LogInformation("[FASE 1] Oportunidades detectadas: {Count}", opportunities.Count);

            if (!opportunities.Any())
            {
                _logger.LogWarning("No se encontraron oportunidades. Pipeline terminado.");
                return;
            }

            // Fase 2: Búsqueda de productos en China
            _logger.LogInformation("[FASE 2] Buscando proveedores en China...");
            await _chinaAgent.FindChineseProductsAsync(opportunities, ct);

            // Fase 3: Cálculo de arbitraje
            _logger.LogInformation("[FASE 3] Calculando márgenes de arbitraje...");
            var pairs = new List<(Domain.Entities.MarketOpportunity, Domain.Entities.ChineseProduct)>();
            foreach (var opp in opportunities)
            {
                var products = await _chineseProductRepo.GetByOpportunityIdAsync(opp.Id, ct);
                pairs.AddRange(products.Select(p => (opp, p)));
            }

            await _calculatorAgent.CalculateProfitabilityAsync(pairs, ct);

            // Fase 4: Gestión de compras
            _logger.LogInformation("[FASE 4] Gestionando órdenes de compra...");
            var approved = await _opportunityRepo.GetByStatusAsync(OpportunityStatus.Approved, ct);
            var approvedCalculations = new List<Domain.Entities.ArbitrageCalculation>();
            foreach (var opp in approved)
            {
                if (opp.Calculation?.IsProfitable == true)
                    approvedCalculations.Add(opp.Calculation);
            }

            await _purchaseAgent.CreateOrdersAsync(approvedCalculations, ct);
            await _purchaseAgent.UpdateOrderStatusesAsync(ct);

            // Fase 5: Publicación en MercadoLibre
            _logger.LogInformation("[FASE 5] Publicando listings en MercadoLibre Chile...");
            var deliveredOrders = await _purchaseOrderRepo.GetByStatusAsync(Domain.Enums.PurchaseStatus.Delivered, ct);
            await _publisherAgent.PublishListingsAsync(deliveredOrders, ct);

            _logger.LogInformation("=== PIPELINE DE ARBITRAJE COMPLETADO === {Time}", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico en pipeline de arbitraje");
            throw;
        }
    }
}
