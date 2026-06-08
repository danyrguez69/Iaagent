#pragma warning disable SKEXP0001, SKEXP0010, SKEXP0101, SKEXP0110

using System.Text.Json;
using IaAgent.Application.Agents.Base;
using IaAgent.Application.Plugins;
using IaAgent.Application.Ports;
using IaAgent.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace IaAgent.Application.Agents;

public sealed class ChinaSearchAgent : AgentBase
{
    public ChinaSearchAgent(
        IKernelFactory kernelFactory,
        ChinaSearchPlugin plugin,
        ILogger<ChinaSearchAgent> logger)
        : base(
            BuildKernel(kernelFactory, plugin),
            logger,
            "BuscadorProductosChina",
            LoadPrompt("IaAgent.Application", "ChinaSearch.md"))
    {
    }

    private static Kernel BuildKernel(IKernelFactory factory, ChinaSearchPlugin plugin)
    {
        var kernel = factory.Create();
        kernel.Plugins.AddFromObject(plugin, "ChinaSearch");
        return kernel;
    }

    public async Task FindChineseProductsAsync(
        List<MarketOpportunity> opportunities,
        CancellationToken ct = default)
    {
        if (!opportunities.Any())
        {
            Logger.LogInformation("[{Agent}] No hay oportunidades para buscar en China", AgentName);
            return;
        }

        var opportunitiesJson = JsonSerializer.Serialize(opportunities.Select(o => new
        {
            o.Id,
            o.ProductTitle,
            o.CurrentChileanPriceClp,
            o.Keywords,
            o.Category
        }));

        var prompt = $"Busca proveedores chinos en AliExpress para las siguientes oportunidades de mercado chileno. " +
                     $"Para cada una, calcula el precio máximo en USD como el 25% del precio chileno dividido por el tipo de cambio actual. " +
                     $"Oportunidades: {opportunitiesJson}";

        await InvokeAgentAsync(prompt, ct);
    }
}
