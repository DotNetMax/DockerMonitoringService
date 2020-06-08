using Newtonsoft.Json;

namespace DockerMonitoringService.Core.JsonModels
{
    public class MemoryStats
    {
        [JsonProperty("usage")]
        public int Usage { get; set; }

        [JsonProperty("max_usage")]
        public int MaxUsage { get; set; }
    }
}