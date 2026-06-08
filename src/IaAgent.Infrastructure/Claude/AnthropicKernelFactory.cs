using IaAgent.Application.Ports;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

namespace IaAgent.Infrastructure.Claude;

public sealed class AnthropicOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string ModelId { get; set; } = "claude-sonnet-4-5";
}

public sealed class AnthropicKernelFactory : IKernelFactory
{
    private readonly AnthropicOptions _options;

    public AnthropicKernelFactory(IOptions<AnthropicOptions> options)
    {
        _options = options.Value;
    }

    public Kernel Create()
    {
        if (string.IsNullOrEmpty(_options.ApiKey))
            throw new InvalidOperationException(
                "La API key de Anthropic no está configurada. " +
                "Configura la variable de entorno Anthropic__ApiKey.");

        var builder = Kernel.CreateBuilder();

        // Usamos el conector OpenAI de SK apuntando al endpoint de Anthropic
        // El SDK Anthropic.SDK está disponible para uso directo si se necesita más control
        builder.AddOpenAIChatCompletion(
            modelId: _options.ModelId,
            apiKey: _options.ApiKey,
            httpClient: CreateAnthropicHttpClient());

        return builder.Build();
    }

    private HttpClient CreateAnthropicHttpClient()
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri("https://api.anthropic.com/v1/")
        };
        client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        client.DefaultRequestHeaders.Add("x-api-key", _options.ApiKey);
        return client;
    }
}
