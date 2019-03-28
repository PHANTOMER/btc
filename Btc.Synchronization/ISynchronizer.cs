using System.Threading.Tasks;

namespace Btc.Synchronization
{
    public interface ISynchronizer
    {
        Task SynchronizeWalletsAsync();
    }
}