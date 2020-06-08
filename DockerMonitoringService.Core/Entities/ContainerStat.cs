using System;

namespace DockerMonitoringService.Core.Entities
{
    public class ContainerStat
    {
        public int Id { get; set; }
        public DateTime StatDate { get; set; }
        public double CPUUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double MemoryUsageMax { get; set; }
        public double NetworkUsage { get; set; }
    }
}