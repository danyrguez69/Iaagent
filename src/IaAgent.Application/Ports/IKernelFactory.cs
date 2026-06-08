using Microsoft.SemanticKernel;

namespace IaAgent.Application.Ports;

public interface IKernelFactory
{
    Kernel Create();
}
