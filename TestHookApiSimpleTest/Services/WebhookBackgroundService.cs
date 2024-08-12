using Microsoft.AspNetCore.SignalR;
using TestHookApiSimpleTest.Models;

namespace TestHookApiSimpleTest.Services
{
    public class WebhookBackgroundService : IHostedService, IDisposable
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private Timer _timer;

        public WebhookBackgroundService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(15));
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            Random random = new Random();
            int randomNumber = random.Next(0, 1000);

            var payload = new List<SimpleDataForHookTest>
            {
                new SimpleDataForHookTest { MyProperty = randomNumber }
            };

            var client = _httpClientFactory.CreateClient();
            var subscribers = PlanningHub.GetSubscribers();

            foreach (var subscriber in subscribers)
            {
                try
                {
                    var response = await client.PostAsJsonAsync(subscriber, payload);
                    response.EnsureSuccessStatusCode();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending webhook: {ex.Message}");
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
