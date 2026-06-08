namespace IaAgent.Domain.Entities;

public class ArbitrageCalculation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OpportunityId { get; set; }
    public Guid ChineseProductId { get; set; }

    public decimal ProductCostUsd { get; set; }
    public decimal ShippingToChileCostUsd { get; set; }
    public decimal CifValueUsd { get; set; }
    public decimal CustomsDutyUsd { get; set; }
    public decimal IvaUsd { get; set; }
    public decimal MercadoLibreFeeUsd { get; set; }
    public decimal TotalCostUsd { get; set; }

    public decimal UsdToClpRate { get; set; }
    public decimal TargetSellingPriceClp { get; set; }
    public decimal TargetSellingPriceUsd { get; set; }

    public decimal GrossProfitUsd { get; set; }
    public decimal GrossMarginPct { get; set; }
    public decimal NetProfitUsd { get; set; }
    public decimal NetMarginPct { get; set; }
    public decimal Roi { get; set; }

    public bool IsProfitable { get; set; }
    public string Rationale { get; set; } = string.Empty;
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}
