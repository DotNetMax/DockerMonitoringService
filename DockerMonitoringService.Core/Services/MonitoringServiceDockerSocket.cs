using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using DockerMonitoringService.Core.Entities;
using DockerMonitoringService.Core.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DockerMonitoringService.Core.Services
{
    public class MonitoringServiceDockerSocket : IMonitoringService
    {
        private MetricsDataContext _metricsDataContext;
        private readonly ILogger<MonitoringServiceDockerSocket> _logger;
        private readonly int _removeAfterHours;
        private readonly DockerClient _dockerClient;


        public MonitoringServiceDockerSocket(int removeAfterHours
            , MetricsDataContext metricsDataContext
            , ILogger<MonitoringServiceDockerSocket> logger)
        {
            _metricsDataContext = metricsDataContext;
            _logger = logger;
            _removeAfterHours = removeAfterHours;

            _dockerClient = new DockerClientConfiguration(new Uri("unix://var/run/docker.sock")).CreateClient();
        }
        
        public Task DeleteOlderContainerStatsEntries()
        {
            var deleteAfterDate = DateTime.Now.ToUniversalTime().AddHours(-_removeAfterHours);
            _logger.LogInformation("Deleting Stats older than {date}", deleteAfterDate);

            var oldContainerStats = _metricsDataContext.ContainerStats.Where(stat=> stat.StatDate <= deleteAfterDate).ToList();
            _logger.LogInformation("{counter} old ContainerStats Entries found", oldContainerStats.Count());

            if(oldContainerStats != null && oldContainerStats.Count > 0)
            {
                _metricsDataContext.ContainerStats.RemoveRange(oldContainerStats);
                _metricsDataContext.SaveChanges();
            }
            return Task.CompletedTask;
        }

        public async Task DeleteNotExistingContainersAsync()
        {
            List<Container> toRemove = new List<Container>();
            foreach(var container in _metricsDataContext.Containers.Include(s=>s.Stats).ToList())
            {
                _logger.LogInformation("Trying to get response from {containerName} from {date}", container.Name, container.Created);
                try
                {             
                    CancellationTokenSource tokenSource = new CancellationTokenSource();
                    tokenSource.CancelAfter(TimeSpan.FromSeconds(3));      
                    var currentState =  await _dockerClient.Containers.InspectContainerAsync(container.DockerContainerId, tokenSource.Token);
                    if(currentState != null)
                    {
                        _logger.LogInformation("OK");
                    }
                }
                catch(Exception ex)
                {                
                    _logger.LogInformation("Container does not exists");
                    toRemove.Add(container);               
                }              
            }

            _logger.LogInformation("{count} not existing containers", toRemove.Count);

            if(toRemove != null && toRemove.Count > 0)
            {               
                foreach(var container in toRemove)
                {                  
                    try 
                    {
                        _logger.LogInformation("Removing {containerName}", container.Name);
                        if(container.Stats.Count > 0)
                        {
                            _metricsDataContext.ContainerStats.RemoveRange(container.Stats);
                            _metricsDataContext.SaveChanges(); 
                        }
                        _metricsDataContext.Containers.Remove(_metricsDataContext.Containers.Where(c=>c.Id == container.Id).FirstOrDefault());
                        _metricsDataContext.SaveChanges(); 
                    }
                    catch(Exception ex)
                    {
                        _logger.LogError(ex, "Remove not working");
                    }                
                }
            }
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
            foreach(var container in _metricsDataContext.Containers.Where(c=>c.State != "stopped").ToList())
            {
                apiDownloadTasks.Add(Task.Run(async ()=>
                {
                    _logger.LogInformation("Loading Stats for Container {containerName}", container.Name);

                    CancellationTokenSource cancellation = new CancellationTokenSource();
                    var stats = await _dockerClient.Containers.GetContainerStatsAsync(container.DockerContainerId
                        , new ContainerStatsParameters() {Stream = false}, cancellation.Token);

                    using(StreamReader reader = new StreamReader(stats))
                    {
                        var containerStat = JsonConvert.DeserializeObject<JsonModels.ContainerStat>(reader.ReadToEnd());
                        results.TryAdd(container.DockerContainerId, containerStat);
                    }           
                }));
            }

            await Task.WhenAll(apiDownloadTasks);

            return results;
        }
        public async Task UpdateContainersAsync()
        {
            _logger.LogInformation("Loading Containers from Docker API");
            var containers = await _dockerClient.Containers.ListContainersAsync(new ContainersListParameters());
            foreach(var container in containers)
            {
                if(!_metricsDataContext.Containers.Any(dbContainer => dbContainer.DockerContainerId == container.ID))
                {
                    _logger.LogInformation("Adding Container {containerName} to Database", container.Names.First().Replace("/", ""));
                    Container newEntry = new Container()
                    {
                        DockerContainerId = container.ID,
                        Created = container.Created,
                        Name = container.Names.First().Replace("/", ""),
                        State = container.State,
                        Stats = new List<ContainerStat>(),
                    };

                    _metricsDataContext.Containers.Add(newEntry);
                }
                else
                {
                    _logger.LogInformation("Updating Container {containerName} to Database", container.Names.First().Replace("/", ""));
                    if(_metricsDataContext.Containers.Where(dbContainer => dbContainer.DockerContainerId == container.ID)
                        .Select(dbContainer => dbContainer.State).First() != container.State)
                    {
                        _metricsDataContext.Containers.Where(dbContainer => dbContainer.DockerContainerId == container.ID)
                            .First().State = container.State;
                    }
                }
            }

            _logger.LogInformation("Checking offline Containers");
            foreach(var container in _metricsDataContext.Containers.ToList())
            {
                if(!containers.Any(c=>c.ID == container.DockerContainerId))
                {
                    container.State = "stopped";
                }
            }

            _logger.LogInformation("Saving Container Data");
            await _metricsDataContext.SaveChangesAsync(); 
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
    }
}