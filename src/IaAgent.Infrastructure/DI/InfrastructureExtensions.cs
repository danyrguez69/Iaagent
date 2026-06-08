using IaAgent.Application.Ports;
using IaAgent.Infrastructure.Adapters;
using IaAgent.Infrastructure.Claude;
using IaAgent.Infrastructure.Http.Resiliency;
using IaAgent.Infrastructure.Persistence;
using IaAgent.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IaAgent.Infrastructure.DI;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Base de datos SQLite
        services.AddDbContext<ArbitrageDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("ArbitrageDb")
                ?? "Data Source=/data/arbitrage.db"));

        // Repositorios
        services.AddScoped<IOpportunityRepository, OpportunityRepository>();
        services.AddScoped<IChineseProductRepository, ChineseProductRepository>();
        services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
        services.AddScoped<IListingRepository, ListingRepository>();

        // HTTP Clients con Polly
        services.AddHttpClient<MercadoLibreAdapter>()
            .AddPolicyHandler(HttpRetryPolicies.GetRetryPolicy())
            .AddPolicyHandler(HttpRetryPolicies.GetCircuitBreakerPolicy());

        services.AddHttpClient<AliExpressAdapter>()
            .AddPolicyHandler(HttpRetryPolicies.GetRetryPolicy());

        services.AddHttpClient<ExchangeRateAdapter>();

        // Adaptadores (implementan Ports)
        services.AddScoped<IMercadoLibrePort, MercadoLibreAdapter>();
        services.AddScoped<IChinaMarketPort, AliExpressAdapter>();
        services.AddScoped<IExchangeRatePort, ExchangeRateAdapter>();

        // Kernel Factory (Claude API via Anthropic)
        services.AddSingleton<IKernelFactory, AnthropicKernelFactory>();

        // Configuración
        services.Configure<AnthropicOptions>(configuration.GetSection("Anthropic"));
        services.Configure<MercadoLibreOptions>(configuration.GetSection("MercadoLibre"));
        services.Configure<AliExpressOptions>(configuration.GetSection("AliExpress"));

        return services;
    }
}
