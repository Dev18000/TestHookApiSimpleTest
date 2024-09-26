# TestHookApiSimpleTest

## Описание

Этот проект представляет собой API для реализации Webhook с использованием ASP.NET Core, SignalR и базы данных SQL Server. Система реагирует на изменения в базе данных, автоматически отправляя обновления подписчикам с помощью SignalR и отправки Webhook уведомлений.

## Основные компоненты

### API:
1. **EventController**:
   - Обрабатывает запросы на подписку и отписку клиентов, а также отправку webhook уведомлений.
   - Основные эндпоинты:
     - `POST /api/event/planning`: отправка данных webhook.
     - `POST /api/event/subscribe`: подписка на обновления.
     - `POST /api/event/unsubscribe`: отписка от обновлений.

### Модели:
1. **SimpleDataForHookTest**: 
   - Модель данных для отправки webhook.
   - Поля: `Name`, `Age`, `OperationType`.

2. **SubscriptionRequest**: 
   - Модель запроса для подписки/отписки.
   - Поле: `Url`.

3. **TestTable**: 
   - Модель таблицы в базе данных.
   - Поля: `Id`, `Name`, `Age`, `LastModifiedAt`.

### SignalR:
1. **UpdateHub**:
   - Управляет подписчиками через SignalR.
   - Обрабатывает подключение, подписку, отписку и отключение клиентов.
   - Хранит список подписчиков в потокобезопасном словаре.

### Служба фоновой обработки:
1. **WebhookBackgroundService**:
   - Использует `SqlTableDependency` для мониторинга изменений в таблице `TestTable` и отправки данных через Webhook.
   - Автоматически запускается и отслеживает изменения в базе данных.

## Используемые технологии и зависимости

- ASP.NET Core
- SignalR для управления подписками и рассылки сообщений.
- SQL Server с поддержкой системных версий для отслеживания истории изменений.
- Библиотека `SqlTableDependency` для мониторинга изменений в таблице.
- `HttpClient` для отправки webhook уведомлений.
  
### Необходимые пакеты
- `System.Data.SqlClient`
- `TableDependency.SqlServer`

## Настройка проекта

### Подготовка базы данных

1. **Создание таблицы с системной версией**:

```
   ALTER TABLE dbo.TestTable
   ADD 
   SysStartTime DATETIME2 GENERATED ALWAYS AS ROW START HIDDEN,
   SysEndTime DATETIME2 GENERATED ALWAYS AS ROW END HIDDEN,
   PERIOD FOR SYSTEM_TIME (SysStartTime, SysEndTime);

   ALTER TABLE dbo.TestTable
   SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.TestTableHistory));
 ```

2. **Чистка временной таблицы**:

```
ALTER TABLE TestTable SET (SYSTEM_VERSIONING = OFF);
DELETE FROM TestTableHistory;
ALTER TABLE TestTable
SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.TestTableHistory, HISTORY_RETENTION_PERIOD = 1 DAY));
```

3. **Пример запроса для получения изменений**:

```
SELECT TOP 1 
    Id, 
    Name, 
    Age, 
    TestTable.LastModifiedAt AS SysStartTime, 
    CAST(NULL AS DATETIME2) AS SysEndTime,
    'INSERT' AS Operation
FROM dbo.TestTable

UNION all

SELECT TOP 1 
    h.Id, 
    h.Name, 
    h.Age, 
    h.SysStartTime, 
    h.SysEndTime, 
    CASE 
        WHEN NOT EXISTS (SELECT 1 FROM dbo.TestTable t WHERE t.Id = h.Id) 
        THEN 'DELETE' 
        ELSE 'UPDATE' 
    END AS Operation
FROM dbo.TestTableHistory h

ORDER BY SysStartTime DESC;
```

## Настройка приложения

1. **Конфигурация CORS: Разрешить любые источники и методы для удобства разработки**:

```
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});
```

2. **Swagger: Включен для тестирования API в режиме разработки**:

```
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

3. **SignalR: SignalR хаб для взаимодействия с клиентами**:

```
app.MapHub<UpdateHub>("/updateHub");
```

## Запуск проекта

1. **Настройте строку подключения к базе данных в WebhookBackgroundService**:

```
_connectionString = "Server=YOUR_SERVER;Database=BaseTest;User ID=YOUR_USER;Password=YOUR_PASSWORD;";
```

3. **Запустите приложение**:

```
dotnet run
```










