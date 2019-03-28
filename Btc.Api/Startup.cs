using Btc.Api.Extensions;
using Btc.Api.Infrastructure;
using Btc.Api.Validation;
using Btc.Bitcoin;
using Btc.Business;
using Btc.Contracts.Configuration;
using Btc.DataAccess.Context;
using Btc.DataAccess.Repositories;
using Btc.Synchronization;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Btc.Api
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            //if (env.IsEnvironment("Development"))
            //{
            //    // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
            //    builder.AddApplicationInsightsSettings(developerMode: true);
            //}

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddFluentValidation(fv =>
                {
                    fv.RunDefaultMvcValidationAfterFluentValidationExecutes = false;
                    fv.RegisterValidatorsFromAssemblyContaining<SendBtcValidator>();
                });

            services.AddDbContext<BtcContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddTransient<IWalletRepository, WalletRepository>();
            services.AddTransient<IBitcoinApiWrapper, BitcoinApiWrapper>();
            services.AddTransient<ITransactionService, TransactionService>();
            services.AddTransient<ISynchronizer, Synchronizer>();

            services.AddHostedService<SchedulerRunnerHostedService>();

            services.AddOptions();
            services.Configure<BitcoinApiConfig>(Configuration.GetSection("BitcoinApiConfig"));
            services.Configure<SynchronizationJobConfig>(Configuration.GetSection("SynchronizationJobConfig"));

            services.UseQuartz(typeof(SynchronizationJob));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<ErrorWrappingMiddleware>();
            app.UseMvc();
        }
    }
}
