using FluentAssertions;
using IaAgent.Domain.Constants;
using IaAgent.Domain.DomainServices;
using IaAgent.Domain.Entities;
using Xunit;

namespace IaAgent.Domain.Tests;

public class ArbitrageDomainServiceTests
{
    private static readonly MarketOpportunity TestOpportunity = new()
    {
        Id = Guid.NewGuid(),
        ProductTitle = "Auriculares Bluetooth",
        CurrentChileanPriceClp = 50_000m
    };

    [Fact]
    public void Calculate_WithProfitableProduct_ReturnsProfitable()
    {
        var product = new ChineseProduct
        {
            Id = Guid.NewGuid(),
            PriceUsd = 5m,
            ShippingCostUsd = 3m,
            SupplierRating = 4.8m
        };

        var result = ArbitrageDomainService.Calculate(
            product, TestOpportunity, usdToClpRate: 950m, targetSellingPriceClp: 50_000m);

        result.IsProfitable.Should().BeTrue();
        result.NetMarginPct.Should().BeGreaterThanOrEqualTo(ChileImportConstants.MinNetMargin);
        result.Roi.Should().BeGreaterThanOrEqualTo(ChileImportConstants.MinRoi);
        result.TotalCostUsd.Should().BePositive();
    }

    [Fact]
    public void Calculate_WithExpensiveProduct_ReturnsNotProfitable()
    {
        var product = new ChineseProduct
        {
            Id = Guid.NewGuid(),
            PriceUsd = 40m,
            ShippingCostUsd = 20m,
            SupplierRating = 4.5m
        };

        var result = ArbitrageDomainService.Calculate(
            product, TestOpportunity, usdToClpRate: 950m, targetSellingPriceClp: 50_000m);

        result.IsProfitable.Should().BeFalse();
    }

    [Fact]
    public void Calculate_AppliesSafetyBuffer()
    {
        var product = new ChineseProduct
        {
            Id = Guid.NewGuid(),
            PriceUsd = 5m,
            ShippingCostUsd = 3m
        };

        var result = ArbitrageDomainService.Calculate(
            product, TestOpportunity, usdToClpRate: 950m, targetSellingPriceClp: 50_000m);

        var expectedCif = (5m + 3m) * ChileImportConstants.SafetyBuffer;
        result.CifValueUsd.Should().Be(expectedCif);
    }

    [Fact]
    public void Calculate_IncludesAllChileanImportCosts()
    {
        var product = new ChineseProduct
        {
            Id = Guid.NewGuid(),
            PriceUsd = 10m,
            ShippingCostUsd = 5m
        };

        var result = ArbitrageDomainService.Calculate(
            product, TestOpportunity, usdToClpRate: 950m, targetSellingPriceClp: 50_000m);

        var cif = (10m + 5m) * ChileImportConstants.SafetyBuffer;
        var duty = cif * ChileImportConstants.ImportDutyRate;
        var iva = (cif + duty) * ChileImportConstants.IvaRate;

        result.CustomsDutyUsd.Should().BeApproximately(duty, 0.01m);
        result.IvaUsd.Should().BeApproximately(iva, 0.01m);
    }

    [Theory]
    [InlineData(5, 3, 950, 50_000, true)]
    [InlineData(30, 15, 950, 50_000, false)]
    [InlineData(2, 1, 900, 30_000, true)]
    public void Calculate_VariousScenarios(
        decimal productCost, decimal shipping, decimal rate, decimal sellingPrice, bool expectedProfitable)
    {
        var product = new ChineseProduct { PriceUsd = productCost, ShippingCostUsd = shipping };
        var result = ArbitrageDomainService.Calculate(product, TestOpportunity, rate, sellingPrice);
        result.IsProfitable.Should().Be(expectedProfitable);
    }

    [Fact]
    public void SuggestSellingPrice_ReturnsRoundedToHundreds()
    {
        var price = ArbitrageDomainService.SuggestSellingPrice(
            totalCostUsd: 10m, usdToClpRate: 950m, minMargin: 0.30m);

        price.Should().BeGreaterThan(0);
        (price % 100).Should().Be(0, "el precio debe redondearse a centenas de CLP");
    }
}
