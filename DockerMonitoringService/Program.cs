using System;
using System.Linq;
using DockerMonitoringService.Core.Services;
using DockerMonitoringService.Core.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace DockerMonitoringService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();
            try
            {
                Log.Information("Service started");
                CreateHostBuilder(args).Build().Run();
            }
            catch(Exception ex)
            {
                Log.Fatal(ex, "Service crashed!");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                    
                    services.AddTransient<IMonitoringService>(
                        s => new MonitoringService(Convert.ToInt32(hostContext.Configuration.GetSection("MonitoringServiceSettings")["RemoveOldEntriesAfterHours"])
                            , s.GetServices<MetricsDataContext>().FirstOrDefault()
                            , s.GetServices<ILogger<MonitoringService>>().FirstOrDefault()
                            , s.GetServices<IDockerEngineAPIClient>().FirstOrDefault()));

                    services.AddTransient<IDockerEngineAPIClient>(
                        s => new DockerEngineAPIClient(s.GetServices<ILogger<DockerEngineAPIClient>>().FirstOrDefault()
                            , hostContext.Configuration.GetSection("MonitoringServiceSettings")["DockerEngineApiLocation"]));

                    var optionsBuilder = new DbContextOptionsBuilder<MetricsDataContext>();
                    optionsBuilder.UseNpgsql(hostContext.Configuration.GetConnectionString("MetricsDbConnection"));                
                    services.AddSingleton<MetricsDataContext>(ctx => new MetricsDataContext(optionsBuilder.Options));      
                })
                .UseSerilog();
    }
}
