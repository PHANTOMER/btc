using System;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;

namespace Btc.Synchronization
{
    public class QuartzJobFactory : IJobFactory, IDisposable
    {
        private readonly IServiceScope _serviceScope;

        public QuartzJobFactory(IServiceProvider serviceProvider)
        {
            _serviceScope = serviceProvider.CreateScope();
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var jobDetail = bundle.JobDetail;
            var job = (IJob) _serviceScope.ServiceProvider.GetRequiredService(jobDetail.JobType);
            return job;
        }

        public void ReturnJob(IJob job) { }

        public void Dispose()
        {
            _serviceScope?.Dispose();
        }
    }
}
