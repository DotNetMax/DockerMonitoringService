# Docker Monitoring Service

You can use this Monitoring Service to Log Metrics of your Docker Instance to a Database (e.g. PostgreSQL) and build a Dashboard using Grafana.

## !UPDATE! 

If you don't want to use the Docker Engine API because of security reasons there is [another](#DockerSocket-Version(Recommended)) way to use this library now. 


#### Requirements

You need to create two tables in your Database. If you use PostgreSQL [here](https://github.com/DotNetMax/DockerMonitoringService/blob/master/DBSQLScripts/CreateTables.sql) is the CREATE Script for the tables. If you use another Database Provider you need to change the Syntax and Datatypes.

#### DockerSocket Version(Recommended)

Use the MonitoringServiceDockerSocket class for the IMonitoringService and it will use the docker socket. You have to mount the docker socket to the container to make this work. Check the [docker-compose.yml](https://github.com/DotNetMax/DockerMonitoringService/blob/master/docker-compose.yml) to see how that works.

#### DockerEngineAPI Version

You need to have the Docker Engine API enabled. [DockerAPI](https://docs.docker.com/engine/api/). Use the MonitoringServiceDockerEngineAPI class for the IMonitoringService interface.
Configure your appsettings: You need to define a Database Connection for Entity Framework Core, the DockerAPI Location and after how many hours old Metrics should be removed. [Here](https://github.com/DotNetMax/DockerMonitoringService/blob/master/DockerMonitoringService/appsettings.json) is an example.


#### How to use

Go to [Nuget](https://www.nuget.org/packages/DockerMonitoringService.Core/) and get the latest Version of the Core package.
Setup Dependency Injection like [this](https://github.com/DotNetMax/DockerMonitoringService/blob/master/DockerMonitoringService/Program.cs). To run the Service use the IMonitoringService interface like [this](https://github.com/DotNetMax/DockerMonitoringService/blob/master/DockerMonitoringService/Worker.cs).

#### Dashboard Example

![image](https://github.com/DotNetMax/DockerMonitoringService/blob/develop/GrafanaExample.png)