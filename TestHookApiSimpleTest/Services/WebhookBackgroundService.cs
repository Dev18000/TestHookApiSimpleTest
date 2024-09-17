using System.Data.SqlClient;
using Microsoft.AspNetCore.SignalR;
using TestHookApiSimpleTest.Models;
using Microsoft.Extensions.Logging;
using System.Data;

namespace TestHookApiSimpleTest.Services
{
    public class WebhookBackgroundService : IHostedService, IDisposable
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _connectionString;
        private SqlConnection _connection;
        private SqlDependency _dependency;
        private readonly ILogger<WebhookBackgroundService> _logger;

        public WebhookBackgroundService(IHttpClientFactory httpClientFactory, ILogger<WebhookBackgroundService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _connectionString = "";
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Start SqlDependency to listen for database changes and open the connection
            SqlDependency.Start(_connectionString);
            _connection = new SqlConnection(_connectionString);
            SetupSqlDependency();
            return Task.CompletedTask;
        }

        // Configures SqlDependency to listen for changes in the specified table/column
        private void SetupSqlDependency()
        {
            if (_connection.State == System.Data.ConnectionState.Closed)
            {
                _connection.Open();
            }

            var command = new SqlCommand("SELECT Age FROM dbo.TestTable", _connection);

            // Create SqlDependency for this command
            _dependency = new SqlDependency(command);
            _dependency.OnChange += OnDependencyChange;

            // Execute the command to enable tracking changes
            using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
            {
                // You can process the result if necessary
            }
        }

        // This method is triggered when a change is detected in the database
        private async void OnDependencyChange(object sender, SqlNotificationEventArgs e)
        {
            _logger.LogInformation("Database change detected!");

            // Get updated data from the database
            var updatedData = new SimpleDataForHookTest
            {
                Name = "Updated Name", // Here you can fetch real data if needed
                Age = GetUpdatedAge() // Fetch updated age from the database
            };

            var payload = new List<SimpleDataForHookTest> { updatedData };

            // Send the webhook notification to all subscribers
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

            // Re-establish the subscription to listen for further changes
            SetupSqlDependency();
        }

        // Fetches the updated 'Age' field from the database
        private int GetUpdatedAge()
        {
            _logger.LogInformation("Fetching updated age from the database.");

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Query to fetch the Age of the user with ID 1
                var query = "SELECT TOP 1 Age FROM dbo.TestTable WHERE Id = 1";

                using (var command = new SqlCommand(query, connection))
                {
                    var result = command.ExecuteScalar();

                    if (result != null && int.TryParse(result.ToString(), out int age))
                    {
                        _logger.LogInformation($"Fetched age: {age}");
                        return age;
                    }
                    else
                    {
                        _logger.LogWarning("No valid age found in the database.");
                        return -1; // Return an error value or handle appropriately
                    }
                }
            }
        }

        // Stop SqlDependency and close the connection
        public Task StopAsync(CancellationToken cancellationToken)
        {
            SqlDependency.Stop(_connectionString);
            _connection.Close();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            SqlDependency.Stop(_connectionString);
            _connection.Dispose();
        }
    }
}
