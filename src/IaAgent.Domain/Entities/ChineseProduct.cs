namespace IaAgent.Domain.Entities;

public class ChineseProduct
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OpportunityId { get; set; }
    public string SourcePlatform { get; set; } = "AliExpress";
    public string ExternalProductId { get; set; } = string.Empty;
    public string ProductUrl { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal PriceUsd { get; set; }
    public decimal MinOrderQuantity { get; set; } = 1;
    public decimal ShippingCostUsd { get; set; }
    public int EstimatedShippingDays { get; set; }
    public decimal SupplierRating { get; set; }
    public int OrderCount { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string ImageUrls { get; set; } = "[]";
    public string Specifications { get; set; } = "{}";
    public DateTime FoundAt { get; set; } = DateTime.UtcNow;
}
