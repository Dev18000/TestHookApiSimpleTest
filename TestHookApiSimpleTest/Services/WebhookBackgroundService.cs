using Microsoft.AspNetCore.SignalR;
using TestHookApiSimpleTest.Controllers;
using TestHookApiSimpleTest.Models;

namespace TestHookApiSimpleTest.Services
{
    /// <summary>
    /// A background service that periodically sends webhook notifications to subscribers.
    /// </summary>
    public class WebhookBackgroundService : IHostedService, IDisposable
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private Timer _timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebhookBackgroundService"/> class.
        /// </summary>
        /// <param name="httpClientFactory">The HTTP client factory to create HTTP clients.</param>
        public WebhookBackgroundService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Starts the background service and sets up the timer to trigger webhook notifications.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(15));
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

            var client = _httpClientFactory.CreateClient();

            foreach (var subscriber in EventController.Subscribers.Values)
            {
                try
                {
                    var response = await client.PostAsJsonAsync(subscriber, payload);
                    response.EnsureSuccessStatusCode();
                }
                catch (Exception ex)
                {
                    // Handle exceptions
                    Console.WriteLine($"Error sending webhook: {ex.Message}");
                }
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
