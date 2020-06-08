using System.Collections.Generic;
using Newtonsoft.Json;

namespace DockerMonitoringService.Core.JsonModels
{
    public class Container
    {
        [JsonProperty("Id")]
        public string Id { get; set; }

        [JsonProperty("Names")]
        public IList<string> Names { get; set; }

        [JsonProperty("Created")]
        public long Created { get; set; }

        [JsonProperty("State")]
        public string State { get; set; }
    }
}