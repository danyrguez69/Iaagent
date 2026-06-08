#pragma warning disable SKEXP0070

using IaAgent.Application.Ports;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

namespace IaAgent.Infrastructure.Google;

public sealed class GeminiOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-3.1-flash-lite-preview";
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/";
}

public sealed class GeminiKernelFactory : IKernelFactory
{
    private readonly GeminiOptions _options;

    public GeminiKernelFactory(IOptions<GeminiOptions> options)
    {
        _options = options.Value;
    }

    public Kernel Create()
    {
        if (string.IsNullOrEmpty(_options.ApiKey))
            throw new InvalidOperationException(
                "La API key de Google AI no está configurada. " +
                "Configura la variable de entorno GoogleAI__ApiKey. " +
                "Obtén una key gratuita en https://aistudio.google.com/apikey");

        var builder = Kernel.CreateBuilder();

        HttpClient? httpClient = !string.IsNullOrEmpty(_options.BaseUrl)
            ? new HttpClient { BaseAddress = new Uri(_options.BaseUrl) }
            : null;

        builder.AddGoogleAIGeminiChatCompletion(
            modelId: _options.Model,
            apiKey: _options.ApiKey,
            httpClient: httpClient);

        return builder.Build();
    }
}
