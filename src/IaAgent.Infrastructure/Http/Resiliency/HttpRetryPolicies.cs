using Polly;
using Polly.Extensions.Http;

namespace IaAgent.Infrastructure.Http.Resiliency;

public static class HttpRetryPolicies
{
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (outcome, timespan, attempt, _) =>
                {
                    Console.WriteLine($"[Retry] Intento {attempt} después de {timespan.TotalSeconds:F1}s. " +
                                      $"Razón: {outcome.Exception?.Message ?? outcome.Result?.ReasonPhrase}");
                });

    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromMinutes(1));
}
