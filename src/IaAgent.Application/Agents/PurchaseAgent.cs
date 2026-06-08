#pragma warning disable SKEXP0001, SKEXP0010, SKEXP0101, SKEXP0110

using System.Text.Json;
using IaAgent.Application.Agents.Base;
using IaAgent.Application.Plugins;
using IaAgent.Application.Ports;
using IaAgent.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace IaAgent.Application.Agents;

public sealed class PurchaseAgent : AgentBase
{
    public PurchaseAgent(
        IKernelFactory kernelFactory,
        PurchasePlugin plugin,
        ILogger<PurchaseAgent> logger)
        : base(
            BuildKernel(kernelFactory, plugin),
            logger,
            "GestorCompras",
            LoadPrompt("IaAgent.Application", "Purchase.md"))
    {
    }

    private static Kernel BuildKernel(IKernelFactory factory, PurchasePlugin plugin)
    {
        var kernel = factory.Create();
        kernel.Plugins.AddFromObject(plugin, "Purchase");
        return kernel;
    }

    public async Task CreateOrdersAsync(
        List<ArbitrageCalculation> approvedCalculations,
        CancellationToken ct = default)
    {
        if (!approvedCalculations.Any())
        {
            Logger.LogInformation("[{Agent}] No hay cálculos aprobados para crear órdenes", AgentName);
            return;
        }

        var calcsJson = JsonSerializer.Serialize(approvedCalculations.Select(c => new
        {
            c.OpportunityId,
            c.ChineseProductId,
            c.ProductCostUsd,
            c.ShippingToChileCostUsd,
            c.TotalCostUsd,
            c.NetMarginPct,
            c.Roi,
            c.Rationale
        }));

        var prompt = $"Crea órdenes de compra para los siguientes cálculos de arbitraje aprobados. " +
                     $"Cada orden debe tener cantidad mínima de 5 unidades. " +
                     $"Cálculos aprobados: {calcsJson}";

        await InvokeAgentAsync(prompt, ct);
    }

    public async Task UpdateOrderStatusesAsync(CancellationToken ct = default)
    {
        var prompt = "Revisa todas las órdenes de compra pendientes con get_pending_orders. " +
                     "Actualiza el estado de las órdenes según corresponda. " +
                     "Marca como Submitted las órdenes en Pending que tengan más de 24 horas.";

        await InvokeAgentAsync(prompt, ct);
    }
}
