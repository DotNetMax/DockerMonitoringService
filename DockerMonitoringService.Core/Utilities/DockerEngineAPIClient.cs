using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using DockerMonitoringService.Core.JsonModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DockerMonitoringService.Core.Utilities
{
    public class DockerEngineAPIClient : IDockerEngineAPIClient
    {
        private readonly ILogger<DockerEngineAPIClient> _logger;
        private readonly string _apiLocation;

        public DockerEngineAPIClient(ILogger<DockerEngineAPIClient> logger, string apiLocation)
        {
            _logger = logger;
            _apiLocation = apiLocation;
        }
        public async Task<IEnumerable<Container>> GetContainersAsync()
        {
            try
            {
                string json = string.Empty;
                using(var webClient = new WebClient())
                {
                    _logger.LogInformation("Requesting {url}", $"{_apiLocation}containers/json");
                    json = await webClient.DownloadStringTaskAsync($"{_apiLocation}containers/json");
                }

                if(!string.IsNullOrEmpty(json))
                {
                    return JsonConvert.DeserializeObject<IEnumerable<Container>>(json);
                }
                else
                {
                    return null;
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to download containers/json {url}", $"{_apiLocation}containers/json");
                return null;
            }
            
        }
        public async Task<ContainerStat> GetContainerStatByIdAsync(string containerId)
        {
            try
            {
                string json = string.Empty;
                using(var webClient = new WebClient())
                {
                    json = await webClient.DownloadStringTaskAsync($"{_apiLocation}containers/{containerId}/stats?stream=false");
                }
                if(!string.IsNullOrEmpty(json))
                {
                    return JsonConvert.DeserializeObject<ContainerStat>(json);
                }
                else
                {
                    return null;
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to donwload stats for container with Id: {containerId}", containerId);
                return null;
            }  
        }

    }
}