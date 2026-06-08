using System.ComponentModel;
using System.Text.Json;
using IaAgent.Application.Ports;
using IaAgent.Domain.Entities;
using IaAgent.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace IaAgent.Application.Plugins;

public sealed class PublisherPlugin
{
    private readonly IMercadoLibrePort _mlPort;
    private readonly IListingRepository _listingRepo;
    private readonly ILogger<PublisherPlugin> _logger;

    public PublisherPlugin(
        IMercadoLibrePort mlPort,
        IListingRepository listingRepo,
        ILogger<PublisherPlugin> logger)
    {
        _mlPort = mlPort;
        _listingRepo = listingRepo;
        _logger = logger;
    }

    [KernelFunction("upload_product_images")]
    [Description("Descarga imágenes desde URLs y las sube a MercadoLibre. Retorna JSON con los IDs de imágenes subidas.")]
    public async Task<string> UploadProductImagesAsync(
        [Description("JSON array de URLs de imágenes a subir")] string imageUrlsJson)
    {
        try
        {
            var urls = JsonSerializer.Deserialize<List<string>>(imageUrlsJson) ?? new();
            var uploadedIds = await _mlPort.UploadImagesAsync(urls);

            _logger.LogInformation("Subidas {Count} imágenes a MercadoLibre", uploadedIds.Count);
            return JsonSerializer.Serialize(new { success = true, uploadedImageIds = uploadedIds });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subiendo imágenes a ML");
            return JsonSerializer.Serialize(new { success = false, uploadedImageIds = new List<string>(), error = ex.Message });
        }
    }

    [KernelFunction("publish_listing")]
    [Description("Crea y publica un listing en MercadoLibre Chile. Retorna el ID del listing creado.")]
    public async Task<string> PublishListingAsync(
        [Description("JSON de MercadoLibreListing con todos los campos requeridos")] string listingJson)
    {
        try
        {
            var listing = JsonSerializer.Deserialize<MercadoLibreListing>(listingJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (listing is null)
                return JsonSerializer.Serialize(new { success = false, error = "JSON inválido" });

            listing.Id = Guid.NewGuid();

            var mlListingId = await _mlPort.CreateListingAsync(listing);
            listing.MercadoLibreListingId = mlListingId;
            listing.Status = ListingStatus.Active;
            listing.PublishedAt = DateTime.UtcNow;

            await _listingRepo.AddAsync(listing);
            await _listingRepo.SaveChangesAsync();

            _logger.LogInformation("Listing publicado en ML: {MlId} - {Title}", mlListingId, listing.Title);
            return JsonSerializer.Serialize(new { success = true, mlListingId, localId = listing.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publicando listing en ML");
            return JsonSerializer.Serialize(new { success = false, error = ex.Message });
        }
    }

    [KernelFunction("update_listing_stock")]
    [Description("Actualiza el stock de un listing activo en MercadoLibre.")]
    public async Task<string> UpdateListingStockAsync(
        [Description("ID del listing en MercadoLibre")] string mlListingId,
        [Description("Nueva cantidad disponible")] int stock)
    {
        try
        {
            var dummyListing = new MercadoLibreListing { Stock = stock };
            await _mlPort.UpdateListingAsync(mlListingId, dummyListing);
            return JsonSerializer.Serialize(new { success = true, mlListingId, newStock = stock });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando stock de {ListingId}", mlListingId);
            return JsonSerializer.Serialize(new { success = false, error = ex.Message });
        }
    }
}
