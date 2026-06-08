namespace IaAgent.Domain.ValueObjects;

public sealed record Margin(decimal NetMarginPct, decimal Roi)
{
    public bool IsViable(decimal minMargin, decimal minRoi) =>
        NetMarginPct >= minMargin && Roi >= minRoi;

    public override string ToString() =>
        $"Margen neto: {NetMarginPct:P1}, ROI: {Roi:P1}";
}
