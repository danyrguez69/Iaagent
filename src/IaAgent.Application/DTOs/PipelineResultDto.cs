namespace IaAgent.Application.DTOs;

public class PipelineResultDto
{
    public int OpportunitiesDetected { get; set; }
    public int ChineseProductsFound { get; set; }
    public int ProfitableCalculations { get; set; }
    public int OrdersCreated { get; set; }
    public int ListingsPublished { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime CompletedAt { get; set; }
    public TimeSpan Duration => CompletedAt - StartedAt;
    public List<string> Errors { get; set; } = new();
    public bool HasErrors => Errors.Count > 0;
}
