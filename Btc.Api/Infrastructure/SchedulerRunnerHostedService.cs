using System;
using System.Threading;
using System.Threading.Tasks;
using Btc.Contracts.Configuration;
using Btc.Synchronization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Spi;

namespace Btc.Api.Infrastructure
{
    public class SchedulerRunnerHostedService : IHostedService
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IJobFactory _jobFactory;
        private readonly ILogger<SchedulerRunnerHostedService> _logger;
        private readonly IOptions<SynchronizationJobConfig> _jobConfig;
        private IScheduler _scheduler;

        public SchedulerRunnerHostedService(
            ISchedulerFactory schedulerFactory, 
            IJobFactory jobFactory, 
            ILogger<SchedulerRunnerHostedService> logger,
            IOptions<SynchronizationJobConfig> jobConfig)
        {
            _schedulerFactory = schedulerFactory;
            _jobFactory = jobFactory;
            _logger = logger;
            _jobConfig = jobConfig;
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogTrace("Starting job scheduler");
            _scheduler = await _schedulerFactory.GetScheduler(cancellationToken).ConfigureAwait(false);
            _scheduler.JobFactory = _jobFactory;
            await _scheduler.Start(cancellationToken).ConfigureAwait(false);
            await StartSynchronizationJob(_scheduler, _jobConfig.Value.Interval).ConfigureAwait(false);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_scheduler == null)
            {
                return;
            }

            _logger.LogTrace("Shutting down job scheduler");
            await _scheduler.Shutdown(cancellationToken).ConfigureAwait(false);
        }

        private static Task<DateTimeOffset> StartSynchronizationJob(IScheduler scheduler, TimeSpan runInterval)
        {
            var jobName = typeof(SynchronizationJob).FullName;

            var job = JobBuilder.Create<SynchronizationJob>()
                .WithIdentity(jobName)
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity($"{jobName}.trigger")
                .StartNow()
                .WithSimpleSchedule(scheduleBuilder =>
                    scheduleBuilder
                        .WithInterval(runInterval)
                        .RepeatForever())
                .Build();

            return scheduler.ScheduleJob(job, trigger);
        }
    }
}