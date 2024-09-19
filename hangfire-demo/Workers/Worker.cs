using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using hangfire_demo.Domain.Interfaces.Services;

namespace hangfire_demo.Workers
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly IServiceProvider _serviceProvider;

        public Worker(ILogger<Worker> logger,
                IBackgroundJobClient backgroundJobClient,
                IRecurringJobManager recurringJobManager,
                IHostApplicationLifetime appLifetime,
                IServiceProvider serviceProvider)
        {
            _logger = logger;
            _backgroundJobClient = backgroundJobClient;
            _recurringJobManager = recurringJobManager;
            _appLifetime = appLifetime;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            _logger.LogInformation("Worker started at: {time}", DateTimeOffset.Now);

            _appLifetime.ApplicationStopping.Register(OnStopping);

            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var jobService = scope.ServiceProvider.GetRequiredService<IJobService>();

                    RecurringJob.AddOrUpdate<IJobService>(
                        "UpdatePanNotifyDlq",
                        job => job.ProcessJobAsync(null),
                        cronExpression: "*/1 * * * *",
                        options: new RecurringJobOptions
                        {
                            TimeZone = TimeZoneInfo.Local
                        });
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Worker stoped at: {time}", DateTimeOffset.Now);

            // Delete all recurring jobs
            _recurringJobManager.RemoveIfExists("UpdatePanNotifyDlq");

            _logger.LogInformation("All jobs cleared.");
            await base.StopAsync(cancellationToken);
        }


        public static void ExecutaUmaVez(PerformContext? context, CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            context.WriteLine($"Metodo executado com sucesso {DateTime.Now}");
        }


        private void OnStopping()
        {
            _logger.LogInformation("Application is stopping. Gracefully shutting down Hangfire...");
            HangfireJobShutdown();
        }

        private void HangfireJobShutdown()
        {
            // Stop processing background jobs
            var backgroundJobServer = new BackgroundJobServer();
            backgroundJobServer.SendStop(); // Request to stop the server and finish running jobs
            backgroundJobServer.WaitForShutdown(TimeSpan.FromSeconds(0)); // Wait for ongoing jobs to finish

            _logger.LogInformation("Hangfire server has been stopped gracefully.");
        }
    }
}
