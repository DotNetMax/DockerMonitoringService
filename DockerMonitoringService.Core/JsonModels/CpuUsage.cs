using Newtonsoft.Json;

namespace DockerMonitoringService.Core.JsonModels
{
    public class CpuUsage
    {
        [JsonProperty("total_usage")]
        public long TotalUsage { get; set; }
    }
}