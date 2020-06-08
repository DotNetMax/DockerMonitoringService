using System;
using Newtonsoft.Json;

namespace DockerMonitoringService.Core.JsonModels
{
    public class ContainerStat
    {
        [JsonProperty("read")]
        public DateTimeOffset Read { get; set; }

        [JsonProperty("cpu_stats")]
        public CpuStats CpuStats { get; set; }

        [JsonProperty("memory_stats")]
        public MemoryStats MemoryStats { get; set; }

        [JsonProperty("networks")]
        public Networks Networks { get; set; }
    }
}