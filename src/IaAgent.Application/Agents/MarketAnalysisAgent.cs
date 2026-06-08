#pragma warning disable SKEXP0001, SKEXP0010, SKEXP0101, SKEXP0110

using IaAgent.Application.Agents.Base;
using IaAgent.Application.Plugins;
using IaAgent.Application.Ports;
using IaAgent.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace IaAgent.Application.Agents;

public sealed class MarketAnalysisAgent : AgentBase
{
    public MarketAnalysisAgent(
        IKernelFactory kernelFactory,
        MarketAnalysisPlugin plugin,
        ILogger<MarketAnalysisAgent> logger)
        : base(
            BuildKernel(kernelFactory, plugin),
            logger,
            "AnalistaMarketChile",
            LoadPrompt("IaAgent.Application", "MarketAnalysis.md"))
    {
    }

    private static Kernel BuildKernel(IKernelFactory factory, MarketAnalysisPlugin plugin)
    {
        var kernel = factory.Create();
        kernel.Plugins.AddFromObject(plugin, "MarketAnalysis");
        return kernel;
    }

    public async Task<List<MarketOpportunity>> AnalyzeMarketAsync(
        List<string> categoryIds,
        CancellationToken ct = default)
    {
        var prompt = $"Analiza las siguientes categorías de MercadoLibre Chile y encuentra oportunidades de arbitraje. " +
                     $"Categorías: {string.Join(", ", categoryIds)}. " +
                     $"Sigue el proceso completo: buscar trending, calcular demand score, filtrar y guardar las oportunidades válidas.";

        await InvokeAgentAsync(prompt, ct);

        Logger.LogInformation("[{Agent}] Análisis de mercado completado para {Count} categorías",
            AgentName, categoryIds.Count);

        return new List<MarketOpportunity>();
    }
}
