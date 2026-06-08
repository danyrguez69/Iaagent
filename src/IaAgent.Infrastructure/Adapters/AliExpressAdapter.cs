using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IaAgent.Application.Ports;
using IaAgent.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IaAgent.Infrastructure.Adapters;

public sealed class AliExpressOptions
{
    public string AppKey { get; set; } = string.Empty;
    public string AppSecret { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
}

public sealed class AliExpressAdapter : IChinaMarketPort
{
    private readonly HttpClient _http;
    private readonly AliExpressOptions _options;
    private readonly ILogger<AliExpressAdapter> _logger;

    public AliExpressAdapter(
        HttpClient http,
        IOptions<AliExpressOptions> options,
        ILogger<AliExpressAdapter> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<List<ChineseProduct>> SearchProductsAsync(
        string keywords,
        decimal maxPriceUsd,
        decimal minRating = 4.5m,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_options.AppKey))
                return await FallbackSearchAsync(keywords, maxPriceUsd, ct);

            var @params = new SortedDictionary<string, string>
            {
                ["method"] = "aliexpress.affiliate.product.query",
                ["app_key"] = _options.AppKey,
                ["session"] = _options.AccessToken,
                ["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
                ["sign_method"] = "hmac-sha256",
                ["v"] = "2.0",
                ["format"] = "json",
                ["keywords"] = keywords,
                ["target_currency"] = "USD",
                ["target_language"] = "en",
                ["page_no"] = "1",
                ["page_size"] = pageSize.ToString(),
                ["max_sale_price"] = ((long)(maxPriceUsd * 100)).ToString(),
                ["sort"] = "SALE_PRICE_ASC"
            };

            @params["sign"] = ComputeSign(@params);

            var queryString = string.Join("&", @params.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
            var response = await _http.GetStringAsync(
                $"https://api-sg.aliexpress.com/sync?{queryString}", ct);

            return ParseProductSearchResponse(response, minRating);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error buscando en AliExpress: {Keywords}", keywords);
            return new List<ChineseProduct>();
        }
    }

    public async Task<decimal> GetShippingCostAsync(
        string productId,
        int quantity,
        string destinationCountry = "CL",
        CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_options.AppKey))
                return 12.0m;

            var @params = new SortedDictionary<string, string>
            {
                ["method"] = "aliexpress.logistics.buyer.freight.calculate",
                ["app_key"] = _options.AppKey,
                ["session"] = _options.AccessToken,
                ["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
                ["sign_method"] = "hmac-sha256",
                ["v"] = "2.0",
                ["format"] = "json",
                ["product_id"] = productId,
                ["product_num"] = quantity.ToString(),
                ["country_code"] = destinationCountry
            };

            @params["sign"] = ComputeSign(@params);
            var queryString = string.Join("&", @params.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
            var response = await _http.GetStringAsync(
                $"https://api-sg.aliexpress.com/sync?{queryString}", ct);

            using var doc = JsonDocument.Parse(response);
            if (doc.RootElement.TryGetProperty("result", out var result) &&
                result.TryGetProperty("freight_list", out var freights) &&
                freights.GetArrayLength() > 0)
            {
                var first = freights[0];
                if (first.TryGetProperty("freight_amount", out var amount))
                    return amount.GetDecimal();
            }

            return 12.0m;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo obtener costo de envío para {ProductId}. Usando estimado.", productId);
            return 12.0m;
        }
    }

    public async Task<ChineseProduct?> GetProductDetailAsync(string productId, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_options.AppKey)) return null;

            var @params = new SortedDictionary<string, string>
            {
                ["method"] = "aliexpress.affiliate.productdetail.get",
                ["app_key"] = _options.AppKey,
                ["session"] = _options.AccessToken,
                ["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
                ["sign_method"] = "hmac-sha256",
                ["v"] = "2.0",
                ["format"] = "json",
                ["product_ids"] = productId,
                ["target_currency"] = "USD",
                ["target_language"] = "en"
            };

            @params["sign"] = ComputeSign(@params);
            var queryString = string.Join("&", @params.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
            var response = await _http.GetStringAsync(
                $"https://api-sg.aliexpress.com/sync?{queryString}", ct);

            using var doc = JsonDocument.Parse(response);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo detalle de producto {ProductId}", productId);
            return null;
        }
    }

    private async Task<List<ChineseProduct>> FallbackSearchAsync(
        string keywords,
        decimal maxPriceUsd,
        CancellationToken ct)
    {
        _logger.LogInformation("Usando búsqueda fallback para AliExpress: {Keywords}", keywords);

        // Cuando no hay API key, retorna lista vacía.
        // En producción, aquí iría el parseo de window.runParams del HTML.
        await Task.CompletedTask;
        return new List<ChineseProduct>();
    }

    private List<ChineseProduct> ParseProductSearchResponse(string response, decimal minRating)
    {
        var results = new List<ChineseProduct>();
        try
        {
            using var doc = JsonDocument.Parse(response);
            if (!doc.RootElement.TryGetProperty("aliexpress_affiliate_product_query_response", out var root))
                return results;
            if (!root.TryGetProperty("resp_result", out var resp)) return results;
            if (!resp.TryGetProperty("result", out var result)) return results;
            if (!result.TryGetProperty("products", out var products)) return results;
            if (!products.TryGetProperty("product", out var productArray)) return results;

            foreach (var item in productArray.EnumerateArray())
            {
                var rating = item.TryGetProperty("evaluate_rate", out var rateEl) &&
                             decimal.TryParse(rateEl.GetString()?.TrimEnd('%'), out var r) ? r / 20m : 0m;

                if (rating < minRating) continue;

                item.TryGetProperty("product_id", out var pid);
                item.TryGetProperty("product_title", out var ptitle);
                item.TryGetProperty("target_sale_price", out var price);
                item.TryGetProperty("store_name", out var storeName);
                item.TryGetProperty("product_main_image_url", out var imgUrl);
                item.TryGetProperty("lastest_volume", out var volume);
                item.TryGetProperty("product_detail_url", out var detailUrl);

                decimal.TryParse(price.GetString(), out var priceValue);

                results.Add(new ChineseProduct
                {
                    ExternalProductId = pid.GetString() ?? string.Empty,
                    Title = ptitle.GetString() ?? string.Empty,
                    PriceUsd = priceValue,
                    SupplierName = storeName.GetString() ?? string.Empty,
                    SupplierRating = rating,
                    ImageUrls = JsonSerializer.Serialize(new[] { imgUrl.GetString() ?? string.Empty }),
                    OrderCount = volume.ValueKind == JsonValueKind.Number ? volume.GetInt32() : 0,
                    ProductUrl = detailUrl.GetString() ?? string.Empty,
                    SourcePlatform = "AliExpress",
                    EstimatedShippingDays = 20,
                    ShippingCostUsd = 12m
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parseando respuesta de AliExpress");
        }
        return results;
    }

    private string ComputeSign(SortedDictionary<string, string> @params)
    {
        var baseString = string.Concat(@params.Select(p => $"{p.Key}{p.Value}"));
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.AppSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(baseString));
        return BitConverter.ToString(hash).Replace("-", "").ToUpper();
    }
}
