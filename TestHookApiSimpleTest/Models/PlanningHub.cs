using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace TestHookApiSimpleTest.Models
{
    public class PlanningHub : Hub
    {
        public static readonly ConcurrentDictionary<string, string> Subscribers = new();

        public override Task OnConnectedAsync()
        {
            Subscribers.TryAdd(Context.ConnectionId, Context.ConnectionId);
            Console.WriteLine($"Client connected: {Context.ConnectionId}");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Subscribers.TryRemove(Context.ConnectionId, out _);
            Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
            return base.OnDisconnectedAsync(exception);
        }

        public async Task Subscribe()
        {
            Subscribers.TryAdd(Context.ConnectionId, Context.ConnectionId);
            Console.WriteLine($"Client subscribed: {Context.ConnectionId}");
        }

        public async Task Unsubscribe()
        {
            Subscribers.TryRemove(Context.ConnectionId, out _);
            Console.WriteLine($"Client unsubscribed: {Context.ConnectionId}");
        }

        public static IReadOnlyCollection<string> GetSubscribers()
        {
            return Subscribers.Keys.ToList();
        }
    }
}
