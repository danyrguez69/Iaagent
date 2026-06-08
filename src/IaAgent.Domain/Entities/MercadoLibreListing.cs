using IaAgent.Domain.Enums;

namespace IaAgent.Domain.Entities;

public class MercadoLibreListing
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OpportunityId { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public string MercadoLibreListingId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public decimal PriceClp { get; set; }
    public int Stock { get; set; }
    public string ConditionType { get; set; } = "new";
    public string ListingType { get; set; } = "gold_special";
    public string ImageUrls { get; set; } = "[]";
    public string Attributes { get; set; } = "[]";
    public ListingStatus Status { get; set; } = ListingStatus.Draft;
    public int TotalSales { get; set; }
    public decimal Revenue { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
