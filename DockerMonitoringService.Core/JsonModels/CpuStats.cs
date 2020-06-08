using Newtonsoft.Json;

namespace DockerMonitoringService.Core.JsonModels
{
    public class CpuStats
    {
        [JsonProperty("cpu_usage")]
        public CpuUsage CpuUsage { get; set; }

        [JsonProperty("system_cpu_usage")]
        public long SystemCpuUsage { get; set; }
    }
}