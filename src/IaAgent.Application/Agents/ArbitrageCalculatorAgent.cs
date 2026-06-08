#pragma warning disable SKEXP0001, SKEXP0010, SKEXP0101, SKEXP0110

using System.Text.Json;
using IaAgent.Application.Agents.Base;
using IaAgent.Application.Plugins;
using IaAgent.Application.Ports;
using IaAgent.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace IaAgent.Application.Agents;

public sealed class ArbitrageCalculatorAgent : AgentBase
{
    public ArbitrageCalculatorAgent(
        IKernelFactory kernelFactory,
        ArbitragePlugin plugin,
        ILogger<ArbitrageCalculatorAgent> logger)
        : base(
            BuildKernel(kernelFactory, plugin),
            logger,
            "CalculadorArbitraje",
            LoadPrompt("IaAgent.Application", "ArbitrageCalc.md"))
    {
    }

    private static Kernel BuildKernel(IKernelFactory factory, ArbitragePlugin plugin)
    {
        var kernel = factory.Create();
        kernel.Plugins.AddFromObject(plugin, "Arbitrage");
        return kernel;
    }

    public async Task<List<ArbitrageCalculation>> CalculateProfitabilityAsync(
        List<(MarketOpportunity Opportunity, ChineseProduct Product)> pairs,
        CancellationToken ct = default)
    {
        if (!pairs.Any())
        {
            Logger.LogInformation("[{Agent}] No hay pares para calcular", AgentName);
            return new List<ArbitrageCalculation>();
        }

        var pairsJson = JsonSerializer.Serialize(pairs.Select(p => new
        {
            opportunity = new
            {
                p.Opportunity.Id,
                p.Opportunity.ProductTitle,
                p.Opportunity.CurrentChileanPriceClp
            },
            product = new
            {
                p.Product.Id,
                p.Product.PriceUsd,
                p.Product.ShippingCostUsd,
                p.Product.SupplierName
            }
        }));

        var prompt = $"Calcula la rentabilidad de arbitraje para los siguientes pares (oportunidad chilena + producto chino). " +
                     $"Aplica TODOS los costos: arancel 6%, IVA 19%, fee ML 12.99%, buffer 10%. " +
                     $"Rechaza cualquier par con margen neto < 25% o ROI < 30%. " +
                     $"Pares a analizar: {pairsJson}";

        await InvokeAgentAsync(prompt, ct);
        return new List<ArbitrageCalculation>();
    }
}
