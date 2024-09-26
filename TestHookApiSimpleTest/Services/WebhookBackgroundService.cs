using System.Data.SqlClient;
using Microsoft.AspNetCore.SignalR;
using TestHookApiSimpleTest.Models;
using Microsoft.Extensions.Logging;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base.Enums;
using TableDependency.SqlClient.Base.EventArgs;

namespace TestHookApiSimpleTest.Services
{
    public class WebhookBackgroundService : IHostedService, IDisposable
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _connectionString;
        private readonly ILogger<WebhookBackgroundService> _logger;
        private SqlTableDependency<TestTable> _tableDependency;

        public WebhookBackgroundService(IHttpClientFactory httpClientFactory, ILogger<WebhookBackgroundService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _connectionString = ""; // todo your connection string
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Webhook service started using SqlTableDependency.");
            StartSqlTableDependency();
            return Task.CompletedTask;
        }

        private void StartSqlTableDependency()
        {
            _tableDependency = new SqlTableDependency<TestTable>(_connectionString);
            _tableDependency.OnChanged += OnTableChanged;
            _tableDependency.OnError += OnTableDependencyError;
            _tableDependency.Start();
        }

        private void OnTableChanged(object sender, RecordChangedEventArgs<TestTable> e)
        {
            if (e.ChangeType == ChangeType.None)
            {
                return;
            }

            var changedEntity = e.Entity;
            var operation = e.ChangeType.ToString().ToUpper();

            var payload = new List<SimpleDataForHookTest>
            {
                new SimpleDataForHookTest
                {
                    Name = changedEntity.Name,
                    Age = changedEntity.Age,
                    OperationType = operation
                }
            };

            // Notify all subscribers
            NotifySubscribers(payload);
            _logger.LogInformation($"Operation {operation} detected for entity with ID: {changedEntity.Id}");
        }

        private void OnTableDependencyError(object sender, TableDependency.SqlClient.Base.EventArgs.ErrorEventArgs e)
        {
            _logger.LogError($"SqlTableDependency error: {e.Error.Message}");
        }

        private async Task NotifySubscribers(List<SimpleDataForHookTest> payload)
        {
            var client = _httpClientFactory.CreateClient();
            var subscribers = UpdateHub.GetSubscribers();

            foreach (var subscriber in subscribers)
            {
                try
                {
                    var response = await client.PostAsJsonAsync(subscriber, payload);
                    response.EnsureSuccessStatusCode();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error sending webhook: {ex.Message}");
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _tableDependency?.Stop();
            _logger.LogInformation("Webhook service stopped.");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _tableDependency?.Dispose();
        }
    }
}
