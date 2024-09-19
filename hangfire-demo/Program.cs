using Hangfire;
using Hangfire.Console;
using Hangfire.Dashboard.BasicAuthorization;
using Hangfire.PostgreSql;
using hangfire_demo.Application.Services;
using hangfire_demo.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Worker = hangfire_demo.Workers.Worker;

namespace hangfire_demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(builder =>
                {
                    builder.Configure((context, app) =>
                    {
                        app.UseRouting();

                        app.UseHangfireDashboard(context.Configuration.GetSection("AppSettings:HangfireSettings:DashboardPath").Value, new DashboardOptions
                        {
                            DashboardTitle = "Hangfire [DTS Jobs] Dashboard",
                            DisplayStorageConnectionString = false,
                            AppPath = context.Configuration.GetSection("AppSettings:HangfireSettings:DashboardPath").Value,
                            DarkModeEnabled = true,
                            FaviconPath = "/favicon.ico",
                            
                            Authorization =
                            [
                                new BasicAuthAuthorizationFilter(
                                    new BasicAuthAuthorizationFilterOptions
                                    {
                                        SslRedirect = false,
                                        RequireSsl = false,
                                        LoginCaseSensitive = true,
                                        Users =
                                        [
                                            new BasicAuthAuthorizationUser
                                            {
                                                Login = context.Configuration.GetSection("AppSettings:HangfireSettings:UserName").Value,
                                                // Password as plain text, SHA1 will be used
                                                PasswordClear = context.Configuration.GetSection("AppSettings:HangfireSettings:Password").Value
                                            }
                                        ]
                                    }
                                )
                            ]
                        });
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapHangfireDashboard(context.Configuration.GetSection("AppSettings:HangfireSettings:DashboardPath").Value);
                        });
                    });
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHangfire(conf =>
                    {
                        conf.SetDataCompatibilityLevel(CompatibilityLevel.Version_170);
                        conf.UseSimpleAssemblyNameTypeSerializer();
                        conf.UseConsole(new ConsoleOptions
                        {
                            BackgroundColor = "#0d3163"
                        });
                        conf.UsePostgreSqlStorage(options => options.UseNpgsqlConnection(hostContext.Configuration.GetConnectionString("DtsDB")), new PostgreSqlStorageOptions
                        {
                            DistributedLockTimeout = TimeSpan.FromMinutes(5)
                        });
                        conf.UseRecommendedSerializerSettings();
                        conf.UseColouredConsoleLogProvider();
                    });
                    services.AddHangfireServer(opt =>
                    {
                        opt.ServerName = hostContext.Configuration.GetSection("AppSettings:HangfireSettings:ServerName").Value;
                        opt.CancellationCheckInterval = TimeSpan.FromSeconds(3);
                        opt.WorkerCount = 1;
                    });

                    services.AddScoped<IJobService, JobService>();
                    services.AddHostedService<Worker>();
                });
    }
}
