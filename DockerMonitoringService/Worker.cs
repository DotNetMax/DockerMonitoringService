using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DockerMonitoringService.Core.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DockerMonitoringService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IMonitoringService _monitoringService;

        public Worker(ILogger<Worker> logger, IMonitoringService monitoringService)
        {
            _logger = logger;
            _monitoringService = monitoringService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Monitoring Service running at: {time}", DateTimeOffset.Now);

                //Load Containers -> Load their Stats -> Delete old Stats -> Delete not existing Containers
                await _monitoringService.UpdateContainersAsync();
                await _monitoringService.SaveCurrentContainerStatsAsync();
                await _monitoringService.DeleteOlderContainerStatsEntries();
                await _monitoringService.DeleteNotExistingContainersAsync();
                //Repeat after 30 Seconds
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}
