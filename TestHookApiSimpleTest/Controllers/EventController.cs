using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using TestHookApiSimpleTest.Models;

namespace TestHookApiSimpleTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public static readonly ConcurrentDictionary<string, string> Subscribers = new();

        public EventController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Send data to subscribers on the planning page
        /// </summary>
        /// <param name="payload">The data to be sent to the subscribers</param>
        /// <returns>Action result indicating the status of the operation</returns>
        [HttpPost("planning")]
        public async Task<IActionResult> SendWebhook([FromBody] IEnumerable<SimpleDataForHookTest> payload)
        {
            return await SendWebhookToSubscribers(payload);
        }

        /// <summary>
        /// Receive a subscribe request and add the subscriber URL to the list
        /// </summary>
        /// <param name="request">The subscription request containing the URL to be subscribed</param>
        /// <returns>Action result indicating the status of the subscription</returns>
        [HttpPost("subscribe")]
        public IActionResult Subscribe([FromBody] SubscriptionRequest request)
        {
            string url = request.Url;
            if (!Subscribers.ContainsKey(url))
            {
                Subscribers[url] = url;
            }
            return Ok(new { status = "subscribed" });
        }

        /// <summary>
        /// Receive an unsubscribe request and remove the subscriber URL from the list
        /// </summary>
        /// <param name="request">The unsubscription request containing the URL to be unsubscribed</param>
        /// <returns>Action result indicating the status of the unsubscription</returns>
        [HttpPost("unsubscribe")]
        public IActionResult Unsubscribe([FromBody] SubscriptionRequest request)
        {
            string url = request.Url;
            Subscribers.TryRemove(url, out _);
            return Ok(new { status = "unsubscribed" });
        }

        /// <summary>
        /// Send the payload data to all subscribed URLs
        /// </summary>
        /// <param name="payload">The data to be sent to the subscribers</param>
        /// <returns>Action result indicating the status of the operation</returns>
        private async Task<IActionResult> SendWebhookToSubscribers(IEnumerable<SimpleDataForHookTest> payload)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                foreach (var subscriber in Subscribers.Values)
                {
                    var response = await client.PostAsJsonAsync(subscriber, payload);
                    response.EnsureSuccessStatusCode();
                }

                return Ok(new { status = "webhook sent" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
