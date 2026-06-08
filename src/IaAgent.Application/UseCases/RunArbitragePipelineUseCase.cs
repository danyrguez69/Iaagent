using IaAgent.Application.Agents;
using IaAgent.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace IaAgent.Application.UseCases;

public sealed class RunArbitragePipelineUseCase
{
    private readonly OrchestratorAgent _orchestrator;
    private readonly ILogger<RunArbitragePipelineUseCase> _logger;

    public RunArbitragePipelineUseCase(
        OrchestratorAgent orchestrator,
        ILogger<RunArbitragePipelineUseCase> logger)
    {
        _orchestrator = orchestrator;
        _logger = logger;
    }

    public async Task<PipelineResultDto> ExecuteAsync(
        PipelineOptions options,
        CancellationToken ct = default)
    {
        var result = new PipelineResultDto { StartedAt = DateTime.UtcNow };

        try
        {
            await _orchestrator.RunPipelineAsync(options.CategoryIds, ct);
            result.CompletedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pipeline falló");
            result.Errors.Add(ex.Message);
            result.CompletedAt = DateTime.UtcNow;
        }

        _logger.LogInformation("Pipeline ejecutado en {Duration}", result.Duration);
        return result;
    }
}
