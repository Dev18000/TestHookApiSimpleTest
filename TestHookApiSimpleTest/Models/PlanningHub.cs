using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace TestHookApiSimpleTest.Models
{
    public class PlanningHub : Hub
    {
        public static readonly ConcurrentDictionary<string, string> Subscribers = new();

        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"Client connected: {Context.ConnectionId}");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Subscribers.TryRemove(Context.ConnectionId, out _);
            Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
            return base.OnDisconnectedAsync(exception);
        }

        public Task Subscribe(string url)
        {
            Subscribers[Context.ConnectionId] = url;
            Console.WriteLine($"Client subscribed: {Context.ConnectionId} with URL: {url}");
            return Task.CompletedTask;
        }

        public Task Unsubscribe()
        {
            Subscribers.TryRemove(Context.ConnectionId, out _);
            Console.WriteLine($"Client unsubscribed: {Context.ConnectionId}");
            return Task.CompletedTask;
        }

        public static IReadOnlyCollection<string> GetSubscribers()
        {
            return Subscribers.Values.ToList();
        }
    }
}
