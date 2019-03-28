using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Btc.Synchronization
{
    [DisallowConcurrentExecution]
    public class SynchronizationJob : IJob
    {
        private readonly ISynchronizer _synchronizer;
        private readonly ILogger<SynchronizationJob> _logger;

        public SynchronizationJob(
            ISynchronizer synchronizer,
            ILogger<SynchronizationJob> logger)
        {
            _synchronizer = synchronizer;
            _logger = logger;
        }
        
        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Synchronization process started");
            await _synchronizer.SynchronizeWalletsAsync().ConfigureAwait(false);
            _logger.LogInformation("Synchronization process completed");
        }
    }
}