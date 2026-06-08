namespace IaAgent.Application.DTOs;

public class PipelineOptions
{
    public List<string> CategoryIds { get; set; } = new()
    {
        "MLC1000",
        "MLC1276",
        "MLC1168",
        "MLC1246",
        "MLC1514"
    };
    public int MaxOpportunitiesPerRun { get; set; } = 10;
    public decimal MinNetMarginPct { get; set; } = 25m;
    public int RunIntervalHours { get; set; } = 6;
}
