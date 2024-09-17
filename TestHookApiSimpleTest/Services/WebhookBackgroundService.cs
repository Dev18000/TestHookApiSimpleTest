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
            // Запускаем SqlDependency и открываем подключение к базе данных
            SqlDependency.Start(_connectionString);
            _connection = new SqlConnection(_connectionString);
            SetupSqlDependency();
            return Task.CompletedTask;
        }

        private void SetupSqlDependency()
        {
            if (_connection.State == System.Data.ConnectionState.Closed)
            {
                _connection.Open();
            }

            var command = new SqlCommand("SELECT Age FROM dbo.TestTable", _connection);

            // Создаем SqlDependency для этого запроса
            _dependency = new SqlDependency(command);
            _dependency.OnChange += OnDependencyChange;

            // Открываем DataReader с использованием правильного поведения
            using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
            {
                // Можно обработать результат, если требуется
            }
        }

        private async void OnDependencyChange(object sender, SqlNotificationEventArgs e)
        {
            _logger.LogInformation("Database change detected!");

            // Получаем обновленные данные из базы
            var updatedData = new SimpleDataForHookTest
            {
                Name = "Updated Name", // Здесь вы можете получить реальные данные, если требуется
                Age = GetUpdatedAge() // Метод для получения обновленного возраста
            };

            var payload = new List<SimpleDataForHookTest> { updatedData };

            // Отправляем вебхук
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

            // Восстанавливаем подписку на изменения
            SetupSqlDependency();
        }

        private int GetUpdatedAge()
        {
            // Логирование начала операции
            _logger.LogInformation("Fetching updated age from the database.");

            // Открываем новое соединение к базе данных
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Пример SQL-запроса для получения возраста по ID
                var query = "SELECT TOP 1 Age FROM dbo.TestTable where Id = 1";  // Можно добавить WHERE, если нужно получать конкретного пользователя

                using (var command = new SqlCommand(query, connection))
                {
                    // Выполняем запрос и получаем результат
                    var result = command.ExecuteScalar();

                    // Проверяем результат и возвращаем возраст
                    if (result != null && int.TryParse(result.ToString(), out int age))
                    {
                        _logger.LogInformation($"Fetched age: {age}");
                        return age;
                    }
                    else
                    {
                        _logger.LogWarning("No valid age found in the database.");
                        return -1; // Или любое другое значение для обработки ошибок
                    }
                }
            }
        }

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
