FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["DockerMonitoringService/DockerMonitoringService.csproj", "DockerMonitoringService/"]
RUN dotnet restore "DockerMonitoringService/DockerMonitoringService.csproj"
COPY . .
WORKDIR "/src/DockerMonitoringService"
RUN dotnet build "DockerMonitoringService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DockerMonitoringService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DockerMonitoringService.dll"]
