using System.Text.Json;
using IaAgent.Application.Ports;
using IaAgent.Domain.Constants;
using Microsoft.Extensions.Logging;

namespace IaAgent.Infrastructure.Adapters;

public sealed class ExchangeRateAdapter : IExchangeRatePort
{
    private readonly HttpClient _http;
    private readonly ILogger<ExchangeRateAdapter> _logger;

    private decimal _cachedRate;
    private DateTime _cacheExpiry = DateTime.MinValue;

    public ExchangeRateAdapter(HttpClient http, ILogger<ExchangeRateAdapter> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<decimal> GetUsdToClpRateAsync(CancellationToken ct = default)
    {
        if (_cachedRate > 0 && DateTime.UtcNow < _cacheExpiry)
        {
            _logger.LogDebug("Usando tipo de cambio cacheado: {Rate}", _cachedRate);
            return _cachedRate;
        }

        try
        {
            var response = await _http.GetStringAsync(ChileImportConstants.ExchangeRateApiUrl, ct);
            using var doc = JsonDocument.Parse(response);

            var clpRate = doc.RootElement
                .GetProperty("rates")
                .GetProperty("CLP")
                .GetDecimal();

            _cachedRate = clpRate;
            _cacheExpiry = DateTime.UtcNow.AddHours(1);

            _logger.LogInformation("Tipo de cambio USD/CLP actualizado: {Rate}", clpRate);
            return clpRate;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo obtener tipo de cambio. Usando valor de fallback 950 CLP/USD");
            return 950m;
        }
    }
}
