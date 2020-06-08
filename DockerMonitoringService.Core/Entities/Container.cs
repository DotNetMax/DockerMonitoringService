using System;
using System.Collections.Generic;

namespace DockerMonitoringService.Core.Entities
{
    public class Container
    {
        public int Id { get; set; }
        public string DockerContainerId { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public string State { get; set; }
        public List<ContainerStat> Stats { get; set; }
    }
}