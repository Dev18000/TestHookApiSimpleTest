using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace TestHookApiSimpleTest.Models
{
    public class UpdateHub : Hub
    {
        // Stores subscribers in a thread-safe dictionary with connection IDs as keys and URLs as values
        public static readonly ConcurrentDictionary<string, string> Subscribers = new();

        // Called when a client connects to the hub
        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"Client connected: {Context.ConnectionId}");
            return base.OnConnectedAsync();
        }

        // Called when a client disconnects from the hub
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            // Removes the client from the subscribers list upon disconnection
            Subscribers.TryRemove(Context.ConnectionId, out _);
            Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
            return base.OnDisconnectedAsync(exception);
        }

        // Allows the client to subscribe with a specific URL
        public Task Subscribe(string url)
        {
            // Adds or updates the subscriber's URL for the current connection ID
            Subscribers[Context.ConnectionId] = url;
            Console.WriteLine($"Client subscribed: {Context.ConnectionId} with URL: {url}");
            return Task.CompletedTask;
        }

        // Allows the client to unsubscribe from updates
        public Task Unsubscribe()
        {
            // Removes the subscriber from the dictionary based on the connection ID
            Subscribers.TryRemove(Context.ConnectionId, out _);
            Console.WriteLine($"Client unsubscribed: {Context.ConnectionId}");
            return Task.CompletedTask;
        }

        // Returns a read-only list of all subscriber URLs
        public static IReadOnlyCollection<string> GetSubscribers()
        {
            return Subscribers.Values.ToList();
        }
    }
}
