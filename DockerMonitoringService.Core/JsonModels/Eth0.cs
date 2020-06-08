using Newtonsoft.Json;

namespace DockerMonitoringService.Core.JsonModels
{
    public class Eth0
    {
        [JsonProperty("rx_bytes")]
        public int RxBytes { get; set; }
    }
}