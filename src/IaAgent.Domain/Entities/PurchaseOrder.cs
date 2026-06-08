using IaAgent.Domain.Enums;

namespace IaAgent.Domain.Entities;

public class PurchaseOrder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OpportunityId { get; set; }
    public Guid ChineseProductId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string SupplierContact { get; set; } = string.Empty;
    public string ProductUrl { get; set; } = string.Empty;
    public int QuantityOrdered { get; set; }
    public decimal UnitCostUsd { get; set; }
    public decimal TotalCostUsd { get; set; }
    public PurchaseStatus Status { get; set; } = PurchaseStatus.Pending;
    public string ExternalOrderId { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public DateTime? OrderPlacedAt { get; set; }
    public DateTime? EstimatedArrival { get; set; }
    public DateTime? ActualArrival { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
