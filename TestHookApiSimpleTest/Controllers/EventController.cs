using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using TestHookApiSimpleTest.Models;

namespace TestHookApiSimpleTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IHubContext<PlanningHub> _hubContext;

        public EventController(IHubContext<PlanningHub> hubContext)
        {
            _hubContext = hubContext;
        }

        /// <summary>
        /// Send data to subscribers on the planning page
        /// </summary>
        /// <param name="payload">The data to be sent to the subscribers</param>
        /// <returns>Action result indicating the status of the operation</returns>
        [HttpPost("planning")]
        public async Task<IActionResult> SendWebhook([FromBody] IEnumerable<SimpleDataForHookTest> payload)
        {
            try
            {
                var subscriberIds = PlanningHub.GetSubscribers().ToList();
                foreach (var subscriberId in subscriberIds)
                {
                    await _hubContext.Clients.Client(subscriberId).SendAsync("ReceivePlanningData", payload);
                }
                return Ok(new { status = "webhook sent" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
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
            PlanningHub.Subscribers[url] = url;
            Console.WriteLine($"Subscribing {url}");
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
            PlanningHub.Subscribers.TryRemove(url, out _);
            Console.WriteLine($"Unsubscribing {url}");
            return Ok(new { status = "unsubscribed" });
        }
    }
}
