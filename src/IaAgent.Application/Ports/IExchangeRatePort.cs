namespace IaAgent.Application.Ports;

public interface IExchangeRatePort
{
    Task<decimal> GetUsdToClpRateAsync(CancellationToken ct = default);
}
