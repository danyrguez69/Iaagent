#pragma warning disable SKEXP0070

using IaAgent.Application.Ports;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

namespace IaAgent.Infrastructure.Google;

public sealed class GeminiOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string ModelId { get; set; } = "gemini-2.0-flash";
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

        builder.AddGoogleAIGeminiChatCompletion(
            modelId: _options.ModelId,
            apiKey: _options.ApiKey);

        return builder.Build();
    }
}
