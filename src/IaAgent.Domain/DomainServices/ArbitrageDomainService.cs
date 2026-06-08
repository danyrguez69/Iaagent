using IaAgent.Domain.Constants;
using IaAgent.Domain.Entities;

namespace IaAgent.Domain.DomainServices;

public static class ArbitrageDomainService
{
    public static ArbitrageCalculation Calculate(
        ChineseProduct product,
        MarketOpportunity opportunity,
        decimal usdToClpRate,
        decimal targetSellingPriceClp)
    {
        var productCostUsd = product.PriceUsd;
        var shippingUsd = product.ShippingCostUsd;

        // CIF con buffer de seguridad del 10%
        var cifUsd = (productCostUsd + shippingUsd) * ChileImportConstants.SafetyBuffer;

        // Arancel 6% sobre CIF
        var dutyUsd = cifUsd * ChileImportConstants.ImportDutyRate;

        // IVA 19% sobre (CIF + Arancel)
        var ivaUsd = (cifUsd + dutyUsd) * ChileImportConstants.IvaRate;

        var sellingPriceUsd = targetSellingPriceClp / usdToClpRate;

        // Comisión ML gold_special 12.99%
        var mlFeeUsd = sellingPriceUsd * ChileImportConstants.MlFeeGoldSpecial;

        var totalCostUsd = cifUsd + dutyUsd + ivaUsd + mlFeeUsd;
        var grossProfitUsd = sellingPriceUsd - (cifUsd + dutyUsd + ivaUsd);
        var netProfitUsd = sellingPriceUsd - totalCostUsd;

        var grossMarginPct = sellingPriceUsd > 0 ? grossProfitUsd / sellingPriceUsd : 0;
        var netMarginPct = sellingPriceUsd > 0 ? netProfitUsd / sellingPriceUsd : 0;
        var roi = totalCostUsd > 0 ? netProfitUsd / totalCostUsd : 0;

        return new ArbitrageCalculation
        {
            OpportunityId = opportunity.Id,
            ChineseProductId = product.Id,
            ProductCostUsd = productCostUsd,
            ShippingToChileCostUsd = shippingUsd,
            CifValueUsd = cifUsd,
            CustomsDutyUsd = dutyUsd,
            IvaUsd = ivaUsd,
            MercadoLibreFeeUsd = mlFeeUsd,
            TotalCostUsd = totalCostUsd,
            UsdToClpRate = usdToClpRate,
            TargetSellingPriceClp = targetSellingPriceClp,
            TargetSellingPriceUsd = sellingPriceUsd,
            GrossProfitUsd = grossProfitUsd,
            GrossMarginPct = grossMarginPct,
            NetProfitUsd = netProfitUsd,
            NetMarginPct = netMarginPct,
            Roi = roi,
            IsProfitable = netMarginPct >= ChileImportConstants.MinNetMargin
                        && roi >= ChileImportConstants.MinRoi
        };
    }

    public static decimal SuggestSellingPrice(decimal totalCostUsd, decimal usdToClpRate, decimal minMargin = 0.30m)
    {
        // Precio mínimo para alcanzar el margen deseado después de la comisión ML
        // netProfit = price - cost - mlFee = price - cost - (price * 0.1299)
        // price * (1 - 0.1299) - cost = price * minMargin
        // price * (1 - 0.1299 - minMargin) = cost
        var divisor = 1m - ChileImportConstants.MlFeeGoldSpecial - minMargin;
        if (divisor <= 0) divisor = 0.1m;
        var minPriceUsd = totalCostUsd / divisor;
        return Math.Ceiling(minPriceUsd * usdToClpRate / 100) * 100; // redondeado a centenas de CLP
    }
}
