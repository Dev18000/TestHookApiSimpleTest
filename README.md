# TestHookApiSimpleTest (API Hook)

Этот проект реализует веб-API, которое отправляет данные клиентам через веб-хуки, используя SignalR для управления подписками.

## Функциональность

- Управление подписками на веб-хуки.
- Отправка данных подписчикам через веб-хуки.
- Использование SignalR для связи с подписчиками.

## Структура проекта

- **Controllers**: `EventController` управляет подписками и отправкой веб-хуков.
- **Hubs**: `PlanningHub` для управления подписками.
- **Services**: `WebhookBackgroundService` периодически отправляет случайные данные подписчикам.

## Установка и запуск

1. Убедитесь, что у вас установлен .NET 6.0 или выше.
2. Клонируйте репозиторий: `git clone <repository-url>`.
3. Перейдите в директорию проекта: `cd TestHookApiSimpleTest`.
4. Установите зависимости и запустите проект: 
   ```bash
   dotnet restore
   dotnet run
   ```
   
## Конфигурация

- API для подписки на веб-хуки: POST /api/Event/subscribe.
- API для отписки от веб-хуков: POST /api/Event/unsubscribe.
- API для отправки данных: POST /api/Event/planning.
  
## Использование

1. Подпишитесь на веб-хук, отправив POST-запрос на /api/Event/subscribe с телом запроса { "Url": "<ваш URL>" }.
2. Отправьте данные подписчикам через веб-хук, отправив POST-запрос на /api/Event/planning с телом запроса, содержащим данные.
3. Отпишитесь от веб-хука, отправив POST-запрос на /api/Event/unsubscribe.
   
## Дополнительная информация

- WebhookBackgroundService автоматически отправляет случайные данные подписчикам каждые 15 секунд для теста и примера
