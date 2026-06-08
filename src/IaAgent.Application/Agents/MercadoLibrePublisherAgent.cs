#pragma warning disable SKEXP0001, SKEXP0010, SKEXP0101, SKEXP0110

using System.Text.Json;
using IaAgent.Application.Agents.Base;
using IaAgent.Application.Plugins;
using IaAgent.Application.Ports;
using IaAgent.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace IaAgent.Application.Agents;

public sealed class MercadoLibrePublisherAgent : AgentBase
{
    public MercadoLibrePublisherAgent(
        IKernelFactory kernelFactory,
        PublisherPlugin plugin,
        ILogger<MercadoLibrePublisherAgent> logger)
        : base(
            BuildKernel(kernelFactory, plugin),
            logger,
            "PublicadorMercadoLibre",
            LoadPrompt("IaAgent.Application", "Publisher.md"))
    {
    }

    private static Kernel BuildKernel(IKernelFactory factory, PublisherPlugin plugin)
    {
        var kernel = factory.Create();
        kernel.Plugins.AddFromObject(plugin, "Publisher");
        return kernel;
    }

    public async Task PublishListingsAsync(
        List<PurchaseOrder> deliveredOrders,
        CancellationToken ct = default)
    {
        if (!deliveredOrders.Any())
        {
            Logger.LogInformation("[{Agent}] No hay órdenes entregadas para publicar", AgentName);
            return;
        }

        var ordersJson = JsonSerializer.Serialize(deliveredOrders.Select(o => new
        {
            o.Id,
            o.OpportunityId,
            o.ChineseProductId,
            o.SupplierName,
            o.ProductUrl,
            o.QuantityOrdered,
            o.UnitCostUsd
        }));

        var prompt = $"Publica listings en MercadoLibre Chile para las siguientes órdenes de compra entregadas. " +
                     $"Crea títulos SEO en español (máx 60 chars), descripciones completas en español chileno. " +
                     $"Usa listing_type=gold_special. NUNCA menciones que el producto viene de China. " +
                     $"Órdenes a publicar: {ordersJson}";

        await InvokeAgentAsync(prompt, ct);
    }
}
