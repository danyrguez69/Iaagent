using IaAgent.Domain.Enums;

namespace IaAgent.Domain.Entities;

public class MarketOpportunity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string MercadoLibreProductId { get; set; } = string.Empty;
    public string ProductTitle { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public decimal CurrentChileanPriceClp { get; set; }
    public decimal DemandScore { get; set; }
    public int SoldLast30Days { get; set; }
    public int ActiveListingsCount { get; set; }
    public string Keywords { get; set; } = "[]";
    public string ThumbnailUrl { get; set; } = string.Empty;
    public OpportunityStatus Status { get; set; } = OpportunityStatus.Detected;
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }

    public List<ChineseProduct> ChineseProducts { get; set; } = new();
    public ArbitrageCalculation? Calculation { get; set; }
}
