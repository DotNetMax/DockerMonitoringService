using System.Threading.Tasks;

namespace DockerMonitoringService.Core.Services
{
    public interface IMonitoringService
    {
        ///<summary>
        ///Adding new Containers to the Containers Table.
        ///Updates the current State on the existing ones.
        ///</summary>
        Task UpdateContainersAsync();

        ///<summary>
        ///Loads Container Stats for all Containers in the Containers Table.
        ///Saves the new Stat-Entry to the ContainerStats Table.
        ///</summary>
        Task SaveCurrentContainerStatsAsync();

        ///<summary>
        ///Deletes older ContainerStats Entries that are older than x hours.
        ///The hour range has to be set in the constructor!
        ///</summary>
        Task DeleteOlderContainerStatsEntries();
    }
}