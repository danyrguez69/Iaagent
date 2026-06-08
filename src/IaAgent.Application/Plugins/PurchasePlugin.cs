using System.ComponentModel;
using System.Text.Json;
using IaAgent.Application.Ports;
using IaAgent.Domain.Entities;
using IaAgent.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace IaAgent.Application.Plugins;

public sealed class PurchasePlugin
{
    private readonly IPurchaseOrderRepository _repo;
    private readonly ILogger<PurchasePlugin> _logger;

    public PurchasePlugin(IPurchaseOrderRepository repo, ILogger<PurchasePlugin> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    [KernelFunction("create_purchase_order")]
    [Description("Crea un registro de orden de compra en la base de datos para un producto chino aprobado. Retorna el ID de la orden creada.")]
    public async Task<string> CreatePurchaseOrderAsync(
        [Description("JSON de PurchaseOrder a crear")] string orderJson)
    {
        try
        {
            var order = JsonSerializer.Deserialize<PurchaseOrder>(orderJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (order is null)
                return JsonSerializer.Serialize(new { success = false, error = "JSON inválido" });

            order.Id = Guid.NewGuid();
            order.Status = PurchaseStatus.Pending;
            order.CreatedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            await _repo.AddAsync(order);
            await _repo.SaveChangesAsync();

            _logger.LogInformation("Orden de compra creada: {Id} - {Supplier} x{Qty}",
                order.Id, order.SupplierName, order.QuantityOrdered);

            return JsonSerializer.Serialize(new { success = true, orderId = order.Id, status = order.Status.ToString() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando orden de compra");
            return JsonSerializer.Serialize(new { success = false, error = ex.Message });
        }
    }

    [KernelFunction("update_order_status")]
    [Description("Actualiza el estado y tracking de una orden de compra existente.")]
    public async Task<string> UpdateOrderStatusAsync(
        [Description("ID de la orden (GUID)")] string orderId,
        [Description("Nuevo estado: Submitted, Confirmed, Shipped, InTransitToChile, CustomsCleared, Delivered, Cancelled")] string status,
        [Description("Número de tracking si está disponible")] string trackingNumber = "")
    {
        try
        {
            var id = Guid.Parse(orderId);
            var order = await _repo.GetByIdAsync(id);
            if (order is null)
                return JsonSerializer.Serialize(new { success = false, error = "Orden no encontrada" });

            order.Status = Enum.Parse<PurchaseStatus>(status, ignoreCase: true);
            order.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(trackingNumber))
                order.TrackingNumber = trackingNumber;

            if (order.Status == PurchaseStatus.Delivered)
                order.ActualArrival = DateTime.UtcNow;

            await _repo.UpdateAsync(order);
            await _repo.SaveChangesAsync();

            return JsonSerializer.Serialize(new { success = true, orderId, newStatus = status });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando orden {OrderId}", orderId);
            return JsonSerializer.Serialize(new { success = false, error = ex.Message });
        }
    }

    [KernelFunction("get_pending_orders")]
    [Description("Retorna todas las órdenes de compra que no han sido entregadas aún.")]
    public async Task<string> GetPendingOrdersAsync()
    {
        try
        {
            var orders = new List<PurchaseOrder>();
            foreach (var status in new[] { PurchaseStatus.Pending, PurchaseStatus.Submitted,
                PurchaseStatus.Confirmed, PurchaseStatus.Shipped, PurchaseStatus.InTransitToChile,
                PurchaseStatus.CustomsCleared })
            {
                orders.AddRange(await _repo.GetByStatusAsync(status));
            }
            return JsonSerializer.Serialize(orders, new JsonSerializerOptions { WriteIndented = false });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo órdenes pendientes");
            return JsonSerializer.Serialize(new List<PurchaseOrder>());
        }
    }

    [KernelFunction("get_delivered_orders")]
    [Description("Retorna todas las órdenes de compra ya entregadas y listas para publicar en MercadoLibre.")]
    public async Task<string> GetDeliveredOrdersAsync()
    {
        try
        {
            var orders = await _repo.GetByStatusAsync(PurchaseStatus.Delivered);
            return JsonSerializer.Serialize(orders, new JsonSerializerOptions { WriteIndented = false });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo órdenes entregadas");
            return JsonSerializer.Serialize(new List<PurchaseOrder>());
        }
    }
}
