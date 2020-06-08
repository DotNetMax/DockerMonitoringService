using System.Collections.Generic;
using System.Threading.Tasks;
using DockerMonitoringService.Core.JsonModels;

namespace DockerMonitoringService.Core.Utilities
{
    public interface IDockerEngineAPIClient
    {
        Task<IEnumerable<Container>> GetContainersAsync();
        Task<ContainerStat> GetContainerStatByIdAsync(string containerId);
    }
}