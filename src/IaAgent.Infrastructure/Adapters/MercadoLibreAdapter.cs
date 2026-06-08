using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using IaAgent.Application.Ports;
using IaAgent.Domain.Constants;
using IaAgent.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IaAgent.Infrastructure.Adapters;

public sealed class MercadoLibreOptions
{
    public string AppId { get; set; } = string.Empty;
    public string AppSecret { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}

public sealed class MercadoLibreAdapter : IMercadoLibrePort
{
    private readonly HttpClient _http;
    private readonly ILogger<MercadoLibreAdapter> _logger;
    private readonly MercadoLibreOptions _options;

    public MercadoLibreAdapter(
        HttpClient http,
        IOptions<MercadoLibreOptions> options,
        ILogger<MercadoLibreAdapter> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
        _http.BaseAddress = new Uri(ChileImportConstants.MercadoLibreBaseUrl);
    }

    public async Task<List<MarketOpportunity>> GetTrendingProductsAsync(
        string categoryId, int limit = 20, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetStringAsync(
                $"/trends/{ChileImportConstants.MercadoLibreChileSiteId}/{categoryId}", ct);

            using var doc = JsonDocument.Parse(response);
            var results = new List<MarketOpportunity>();

            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in doc.RootElement.EnumerateArray().Take(limit))
                {
                    results.Add(new MarketOpportunity
                    {
                        MercadoLibreProductId = item.TryGetProperty("id", out var id) ? id.GetString() ?? "" : "",
                        ProductTitle = item.TryGetProperty("keyword", out var kw) ? kw.GetString() ?? "" : "",
                        CategoryId = categoryId,
                        DemandScore = 70m
                    });
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo trending products para categoría {CategoryId}", categoryId);
            return new List<MarketOpportunity>();
        }
    }

    public async Task<List<MarketOpportunity>> SearchProductsAsync(
        string query, int limit = 50, CancellationToken ct = default)
    {
        try
        {
            var url = $"/sites/{ChileImportConstants.MercadoLibreChileSiteId}/search" +
                      $"?q={Uri.EscapeDataString(query)}&sort=sold_quantity_desc&limit={limit}";

            var response = await _http.GetStringAsync(url, ct);
            using var doc = JsonDocument.Parse(response);

            var results = new List<MarketOpportunity>();
            if (doc.RootElement.TryGetProperty("results", out var items))
            {
                foreach (var item in items.EnumerateArray())
                {
                    results.Add(MapToOpportunity(item));
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error buscando productos con query: {Query}", query);
            return new List<MarketOpportunity>();
        }
    }

    public async Task<decimal> GetProductDemandScoreAsync(string itemId, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetStringAsync($"/items/{itemId}", ct);
            using var doc = JsonDocument.Parse(response);

            var soldQty = doc.RootElement.TryGetProperty("sold_quantity", out var sold)
                ? sold.GetInt32() : 0;
            var activeListings = doc.RootElement.TryGetProperty("available_quantity", out var avail)
                ? avail.GetInt32() : 0;

            var score = Math.Min(100m, soldQty * 2m + (activeListings > 0 ? 10m : 0m));
            return score;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo obtener demand score para {ItemId}", itemId);
            return 0m;
        }
    }

    public async Task<string> CreateListingAsync(MercadoLibreListing listing, CancellationToken ct = default)
    {
        try
        {
            SetAuthHeader();
            var imageIds = JsonSerializer.Deserialize<List<string>>(listing.ImageUrls) ?? new();

            var payload = new
            {
                title = listing.Title,
                category_id = listing.CategoryId,
                price = listing.PriceClp,
                currency_id = "CLP",
                available_quantity = listing.Stock,
                buying_mode = "buy_it_now",
                condition = listing.ConditionType,
                listing_type_id = listing.ListingType,
                description = new { plain_text = listing.Description },
                pictures = imageIds.Select(id => new { id }).ToArray(),
                site_id = ChileImportConstants.MercadoLibreChileSiteId
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync("/items", content, ct);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(responseBody);
            return doc.RootElement.GetProperty("id").GetString() ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando listing en ML: {Title}", listing.Title);
            throw;
        }
    }

    public async Task UpdateListingAsync(string listingId, MercadoLibreListing listing, CancellationToken ct = default)
    {
        try
        {
            SetAuthHeader();
            var payload = new { available_quantity = listing.Stock };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PutAsync($"/items/{listingId}", content, ct);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando listing {ListingId}", listingId);
            throw;
        }
    }

    public async Task<List<string>> UploadImagesAsync(List<string> imageUrls, CancellationToken ct = default)
    {
        var uploadedIds = new List<string>();
        SetAuthHeader();

        foreach (var url in imageUrls.Take(10))
        {
            try
            {
                var imageBytes = await _http.GetByteArrayAsync(url, ct);
                using var content = new MultipartFormDataContent();
                using var imageContent = new ByteArrayContent(imageBytes);
                imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                content.Add(imageContent, "file", "product.jpg");

                var response = await _http.PostAsync("/pictures/items/upload", content, ct);
                if (response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync(ct);
                    using var doc = JsonDocument.Parse(body);
                    var id = doc.RootElement.GetProperty("id").GetString();
                    if (id is not null) uploadedIds.Add(id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo subir imagen: {Url}", url);
            }
        }

        return uploadedIds;
    }

    public async Task<bool> RefreshTokenAsync(CancellationToken ct = default)
    {
        try
        {
            var payload = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["client_id"] = _options.AppId,
                ["client_secret"] = _options.AppSecret,
                ["refresh_token"] = _options.RefreshToken
            };

            var response = await _http.PostAsync(
                "/oauth/token",
                new FormUrlEncodedContent(payload),
                ct);

            if (!response.IsSuccessStatusCode) return false;

            var body = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(body);
            _options.AccessToken = doc.RootElement.GetProperty("access_token").GetString() ?? string.Empty;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refrescando token de MercadoLibre");
            return false;
        }
    }

    private void SetAuthHeader()
    {
        if (!string.IsNullOrEmpty(_options.AccessToken))
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _options.AccessToken);
    }

    private static MarketOpportunity MapToOpportunity(JsonElement item)
    {
        item.TryGetProperty("id", out var id);
        item.TryGetProperty("title", out var title);
        item.TryGetProperty("price", out var price);
        item.TryGetProperty("category_id", out var catId);
        item.TryGetProperty("sold_quantity", out var sold);
        item.TryGetProperty("thumbnail", out var thumb);

        return new MarketOpportunity
        {
            MercadoLibreProductId = id.GetString() ?? string.Empty,
            ProductTitle = title.GetString() ?? string.Empty,
            CurrentChileanPriceClp = price.ValueKind == JsonValueKind.Number ? price.GetDecimal() : 0,
            CategoryId = catId.GetString() ?? string.Empty,
            SoldLast30Days = sold.ValueKind == JsonValueKind.Number ? sold.GetInt32() : 0,
            ThumbnailUrl = thumb.GetString() ?? string.Empty,
            DemandScore = 0
        };
    }
}
