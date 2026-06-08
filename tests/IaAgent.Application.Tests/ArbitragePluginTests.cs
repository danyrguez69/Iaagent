using FluentAssertions;
using IaAgent.Application.Plugins;
using IaAgent.Application.Ports;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace IaAgent.Application.Tests;

public class ArbitragePluginTests
{
    private readonly Mock<IExchangeRatePort> _exchangePortMock = new();
    private readonly ArbitragePlugin _plugin;

    public ArbitragePluginTests()
    {
        _exchangePortMock
            .Setup(x => x.GetUsdToClpRateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(950m);

        _plugin = new ArbitragePlugin(
            _exchangePortMock.Object,
            NullLogger<ArbitragePlugin>.Instance);
    }

    [Fact]
    public async Task GetUsdClpRate_ReturnsValidJson()
    {
        var result = await _plugin.GetUsdClpRateAsync();

        result.Should().Contain("usdToClp");
        result.Should().Contain("950");
    }

    [Fact]
    public async Task SuggestOptimalPrice_ReturnsSuggestedPrice()
    {
        var result = await _plugin.SuggestOptimalPriceAsync(
            totalCostUsd: 10m,
            competitorAvgPriceClp: 80_000m,
            minMarginPct: 0.30m);

        result.Should().Contain("suggestedPriceClp");
        result.Should().NotContain("error");
    }

    [Fact]
    public async Task CalculateArbitrageMargin_WithInvalidJson_ReturnsError()
    {
        var result = await _plugin.CalculateArbitrageMarginAsync(
            chineseProductJson: "invalid json",
            opportunityJson: "{}",
            targetSellingPriceClp: 50_000m);

        result.Should().Contain("error");
    }
}
