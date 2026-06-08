using IaAgent.Application.Agents;
using IaAgent.Application.DTOs;
using IaAgent.Application.Plugins;
using IaAgent.Application.UseCases;
using IaAgent.Infrastructure.DI;
using IaAgent.Infrastructure.Persistence;
using IaAgent.Worker.Workers;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Iniciando IaAgent - Sistema Multi-Agente de Arbitraje Chile-China");

    var builder = Host.CreateApplicationBuilder(args);

    // Serilog desde configuración
    builder.Services.AddSerilog((services, lc) => lc
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // Infraestructura (Onion: capa externa)
    builder.Services.AddInfrastructure(builder.Configuration);

    // Plugins (Application layer)
    builder.Services.AddScoped<MarketAnalysisPlugin>();
    builder.Services.AddScoped<ChinaSearchPlugin>();
    builder.Services.AddScoped<ArbitragePlugin>();
    builder.Services.AddScoped<PurchasePlugin>();
    builder.Services.AddScoped<PublisherPlugin>();

    // Agentes especializados (Application layer)
    builder.Services.AddScoped<MarketAnalysisAgent>();
    builder.Services.AddScoped<ChinaSearchAgent>();
    builder.Services.AddScoped<ArbitrageCalculatorAgent>();
    builder.Services.AddScoped<PurchaseAgent>();
    builder.Services.AddScoped<MercadoLibrePublisherAgent>();
    builder.Services.AddScoped<OrchestratorAgent>();

    // Caso de uso
    builder.Services.AddScoped<RunArbitragePipelineUseCase>();

    // Configuración del pipeline
    builder.Services.Configure<PipelineOptions>(builder.Configuration.GetSection("Pipeline"));

    // Worker background service
    builder.Services.AddHostedService<ArbitragePipelineWorker>();

    var host = builder.Build();

    // Ejecutar migraciones de EF Core al arrancar
    using (var scope = host.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ArbitrageDbContext>();
        Log.Information("Ejecutando migraciones de base de datos...");
        await db.Database.MigrateAsync();
        Log.Information("Migraciones completadas.");
    }

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Error fatal al iniciar IaAgent");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

return 0;
