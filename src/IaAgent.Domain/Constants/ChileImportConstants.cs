namespace IaAgent.Domain.Constants;

public static class ChileImportConstants
{
    public const decimal ImportDutyRate = 0.06m;
    public const decimal IvaRate = 0.19m;
    public const decimal MlFeeGoldSpecial = 0.1299m;
    public const decimal MlFeeGoldPro = 0.1699m;
    public const decimal SafetyBuffer = 1.10m;
    public const decimal MinNetMargin = 0.25m;
    public const decimal MinRoi = 0.30m;
    public const int RecommendedMinOrderQty = 5;
    public const int MaxShippingDays = 30;
    public const string MercadoLibreChileSiteId = "MLC";
    public const string MercadoLibreBaseUrl = "https://api.mercadolibre.com";
    public const string AliExpressApiBaseUrl = "https://api-sg.aliexpress.com/sync";
    public const string ExchangeRateApiUrl = "https://open.er-api.com/v6/latest/USD";
}
