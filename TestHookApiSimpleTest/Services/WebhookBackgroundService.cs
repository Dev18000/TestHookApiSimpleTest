using Microsoft.AspNetCore.SignalR;
using TestHookApiSimpleTest.Models;

namespace TestHookApiSimpleTest.Services
{
    /// <summary>
    /// A background service that periodically sends webhook notifications to subscribers.
    /// </summary>
    public class WebhookBackgroundService : IHostedService, IDisposable
    {
        private readonly IHubContext<PlanningHub> _hubContext;
        private Timer _timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebhookBackgroundService"/> class.
        /// </summary>
        /// <param name="hubContext">The SignalR hub context.</param>
        public WebhookBackgroundService(IHubContext<PlanningHub> hubContext)
        {
            _hubContext = hubContext;
        }

        /// <summary>
        /// Starts the background service and sets up the timer to trigger webhook notifications.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(55));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sends webhook notifications to all subscribers.
        /// </summary>
        /// <param name="state">The state object (not used).</param>
        private async void DoWork(object state)
        {
            var payload = new List<SimpleDataForHookTest>
            {
                new SimpleDataForHookTest
                {
                    // Populate with your test data
                   MyProperty = 456
                }
            };

            try
            {
                await _hubContext.Clients.All.SendAsync("ReceivePlanningData", payload);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error sending data via SignalR: {ex.Message}");
            }
        }

        /// <summary>
        /// Stops the background service and disposes of the timer.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Disposes of the resources used by the background service.
        /// </summary>
        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
