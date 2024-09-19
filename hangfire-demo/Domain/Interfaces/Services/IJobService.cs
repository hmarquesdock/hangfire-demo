using Hangfire;
using Hangfire.Server;
using System.ComponentModel;

namespace hangfire_demo.Domain.Interfaces.Services
{
    public interface IJobService
    {
        [AutomaticRetry(Attempts = 0, DelaysInSeconds = [300], LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        [DisplayName("ProcessJobAsyncNotificationPan")]
        Task ProcessJobAsync(PerformContext context);
    }
}
