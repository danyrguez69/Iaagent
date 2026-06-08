#pragma warning disable SKEXP0001, SKEXP0010, SKEXP0101, SKEXP0110

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace IaAgent.Application.Agents.Base;

public abstract class AgentBase
{
    protected readonly Kernel Kernel;
    protected readonly ILogger Logger;
    protected readonly string AgentName;
    private readonly string _systemPrompt;

    protected AgentBase(Kernel kernel, ILogger logger, string agentName, string systemPrompt)
    {
        Kernel = kernel;
        Logger = logger;
        AgentName = agentName;
        _systemPrompt = systemPrompt;
    }

    protected ChatCompletionAgent BuildAgent()
    {
        return new ChatCompletionAgent
        {
            Name = AgentName,
            Instructions = _systemPrompt,
            Kernel = Kernel,
            Arguments = new KernelArguments(
                new OpenAIPromptExecutionSettings
                {
                    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
                    Temperature = 0.1,
                    MaxTokens = 4096
                })
        };
    }

    protected async Task<string> InvokeAgentAsync(string userMessage, CancellationToken ct = default)
    {
        var agent = BuildAgent();
        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage(userMessage);

        Logger.LogInformation("[{Agent}] Iniciando tarea: {Message}",
            AgentName, userMessage[..Math.Min(100, userMessage.Length)]);

        var responses = new List<string>();
        await foreach (var response in agent.InvokeAsync(chatHistory).WithCancellation(ct))
        {
            if (response.Content is not null)
                responses.Add(response.Content);
        }

        var result = string.Join("\n", responses);
        Logger.LogInformation("[{Agent}] Tarea completada. Respuesta: {Length} chars",
            AgentName, result.Length);
        return result;
    }

    protected static string LoadPrompt(string projectName, string filename)
    {
        // Busca el prompt en el directorio de output del assembly
        var basePath = Path.Combine(AppContext.BaseDirectory, "Prompts", filename);
        if (File.Exists(basePath)) return File.ReadAllText(basePath);

        // Fallback para desarrollo
        var devPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..", "..",
            "src", projectName, "Prompts", filename);
        var fullDevPath = Path.GetFullPath(devPath);
        if (File.Exists(fullDevPath)) return File.ReadAllText(fullDevPath);

        return $"Eres el agente {filename.Replace(".md", "")}. Ejecuta tu tarea usando las funciones disponibles.";
    }
}
