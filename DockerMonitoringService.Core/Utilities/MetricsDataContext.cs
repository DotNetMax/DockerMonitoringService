using DockerMonitoringService.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DockerMonitoringService.Core.Utilities
{
    public class MetricsDataContext : DbContext
    {
        public MetricsDataContext(DbContextOptions<MetricsDataContext> options): base(options)
        {
            
        }
        
        public DbSet<Container> Containers { get; set; }
        public DbSet<ContainerStat> ContainerStats { get; set; }

    }
}