using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DockerMonitoringService.Core.Entities;
using DockerMonitoringService.Core.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DockerMonitoringService.Core.Services
{
    public class MonitoringService : IMonitoringService
    {
        private readonly IDockerEngineAPIClient _apiClient;
        private MetricsDataContext _metricsDataContext;
        private readonly ILogger<MonitoringService> _logger;
        private readonly int _removeAfterHours;

        public MonitoringService(int removeAfterHours
            , MetricsDataContext metricsDataContext
            , ILogger<MonitoringService> logger
            , IDockerEngineAPIClient apiClient)
        {
            _apiClient = apiClient;
            _metricsDataContext = metricsDataContext;
            _logger = logger;
            _removeAfterHours = removeAfterHours;
        }
       
        public async Task UpdateContainersAsync()
        {
            _logger.LogInformation("Loading Containers from Docker API");
            var containers = await _apiClient.GetContainersAsync();
            foreach(var container in containers)
            {
                if(!_metricsDataContext.Containers.Any(dbContainer => dbContainer.DockerContainerId == container.Id))
                {
                    _logger.LogInformation($"Adding Container {container.Names.First().Replace("/", "")} to Database");
                    Container newEntry = new Container()
                    {
                        DockerContainerId = container.Id,
                        Created = DateTimeOffset.FromUnixTimeSeconds(container.Created).UtcDateTime,
                        Name = container.Names.First().Replace("/", ""),
                        State = container.State,
                        Stats = new List<ContainerStat>(),
                    };

                    _metricsDataContext.Containers.Add(newEntry);
                }
                else
                {
                    _logger.LogInformation($"Updating Container {container.Names.First().Replace("/", "")}");
                    if(_metricsDataContext.Containers.Where(dbContainer => dbContainer.DockerContainerId == container.Id)
                        .Select(dbContainer => dbContainer.State).First() != container.State)
                    {
                        _metricsDataContext.Containers.Where(dbContainer => dbContainer.DockerContainerId == container.Id)
                            .First().State = container.State;
                    }
                }
            }

            _logger.LogInformation("Checking offline Containers");
            foreach(var container in _metricsDataContext.Containers.ToList())
            {
                if(!containers.Any(c=>c.Id == container.DockerContainerId))
                {
                    container.State = "stopped";
                }
            }

            _logger.LogInformation("Saving Container Data");
            await _metricsDataContext.SaveChangesAsync();
        }       
        public async Task SaveCurrentContainerStatsAsync()
        {
            var statDate = DateTime.Now.ToUniversalTime();
            var containerStats = await GetContainerStatsParralelAsync();

            foreach(var container in _metricsDataContext.Containers.Include(c=>c.Stats).ToList())
            {
                JsonModels.ContainerStat currentStats = new JsonModels.ContainerStat();
                containerStats.TryGetValue(container.DockerContainerId, out currentStats);
                if(currentStats != null && currentStats.Read.Year == DateTime.Now.Year)
                {
                    ContainerStat newStatEntry = new ContainerStat()
                    {
                        StatDate = statDate,
                        CPUUsage = GetCPUUsageInPercentage(currentStats.CpuStats.CpuUsage.TotalUsage, currentStats.CpuStats.SystemCpuUsage),
                        MemoryUsageMax = GetMegabytesFromBytes(currentStats.MemoryStats.MaxUsage),
                        MemoryUsage = GetMegabytesFromBytes(currentStats.MemoryStats.Usage),
                        NetworkUsage = GetMegabytesFromBytes(currentStats.Networks.Eth0.RxBytes)
                    };
                    try
                    {
                        container.Stats.Add(newStatEntry);
                        _logger.LogInformation("Added {containerName} Stats", container.Name);
                    }
                    catch(Exception ex)
                    {
                        _logger.LogError(ex, "Could not add Stats to Container {containerName}", container.Name);
                    }
                    
                }
            }
            _logger.LogInformation("Saving Container Stats");
            await _metricsDataContext.SaveChangesAsync();
        }
        private async Task<ConcurrentDictionary<string, JsonModels.ContainerStat>> GetContainerStatsParralelAsync()
        {
            ConcurrentDictionary<string, JsonModels.ContainerStat> results = new ConcurrentDictionary<string, JsonModels.ContainerStat>();
            List<Task> apiDownloadTasks = new List<Task>();
            foreach(var container in _metricsDataContext.Containers.ToList())
            {
                apiDownloadTasks.Add(Task.Run(async ()=>
                {
                    _logger.LogInformation("Loading Stats for Container {containerName}", container.Name);
                    var containerStats = await _apiClient.GetContainerStatByIdAsync(container.DockerContainerId);
                    results.TryAdd(container.DockerContainerId, containerStats);
                }));
            }

            await Task.WhenAll(apiDownloadTasks);

            return results;
        }
        private double GetMegabytesFromBytes(int bytes)
        {
            return Math.Round(Convert.ToDouble(bytes) / 1024 / 1024, 2);
        }
        private double GetCPUUsageInPercentage(long currentCPUUsage, long systemCPU)
        {
            double percentage = (Convert.ToDouble(currentCPUUsage) / Convert.ToDouble(systemCPU)) * 100;
            return Convert.ToDouble(Math.Round(percentage, 2));
        }

        public Task DeleteOlderContainerStatsEntries()
        {
            var deleteAfterDate = DateTime.Now.ToUniversalTime().AddHours(-_removeAfterHours);
            var oldContainerStats = _metricsDataContext.ContainerStats.Where(stat=> stat.StatDate <= deleteAfterDate).ToList();

            _logger.LogInformation("{counter} old ContainerStats Entries found", oldContainerStats.Count());

            if(oldContainerStats != null && oldContainerStats.Count > 0)
            {
                _metricsDataContext.ContainerStats.RemoveRange(oldContainerStats);
                _metricsDataContext.SaveChanges();
            }
            return Task.CompletedTask;
        }
    }
}