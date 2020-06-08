using Newtonsoft.Json;

namespace DockerMonitoringService.Core.JsonModels
{
    public class Networks
    {
        [JsonProperty("eth0")]
        public Eth0 Eth0 { get; set; }
    }
}