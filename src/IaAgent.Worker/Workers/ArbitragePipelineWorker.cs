using IaAgent.Application.DTOs;
using IaAgent.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IaAgent.Worker.Workers;

public sealed class ArbitragePipelineWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ArbitragePipelineWorker> _logger;
    private readonly PipelineOptions _options;

    public ArbitragePipelineWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<PipelineOptions> options,
        ILogger<ArbitragePipelineWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "ArbitragePipelineWorker iniciado. Intervalo: {Hours}h, Categorías: {Cats}",
            _options.RunIntervalHours,
            string.Join(", ", _options.CategoryIds));

        // Esperar 30 segundos al arrancar para que el sistema se estabilice
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Iniciando ciclo de pipeline [{Time}]", DateTime.UtcNow);

                using var scope = _scopeFactory.CreateScope();
                var useCase = scope.ServiceProvider.GetRequiredService<RunArbitragePipelineUseCase>();
                var result = await useCase.ExecuteAsync(_options, stoppingToken);

                _logger.LogInformation(
                    "Ciclo completado en {Duration}. Oportunidades: {Opp}, Cálculos rentables: {Calc}, Listings publicados: {Pub}",
                    result.Duration,
                    result.OpportunitiesDetected,
                    result.ProfitableCalculations,
                    result.ListingsPublished);

                if (result.HasErrors)
                    _logger.LogWarning("Errores en el ciclo: {Errors}", string.Join("; ", result.Errors));
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ciclo de pipeline. Reintentando en {Hours}h", _options.RunIntervalHours);
            }

            try
            {
                await Task.Delay(TimeSpan.FromHours(_options.RunIntervalHours), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("ArbitragePipelineWorker detenido.");
    }
}
