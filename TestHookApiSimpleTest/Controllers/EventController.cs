using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TestHookApiSimpleTest.Models;

namespace TestHookApiSimpleTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHubContext<UpdateHub> _hubContext;

        public EventController(IHttpClientFactory httpClientFactory, IHubContext<UpdateHub> hubContext)
        {
            _httpClientFactory = httpClientFactory;
            _hubContext = hubContext;
        }

        [HttpPost("planning")]
        public async Task<IActionResult> SendWebhook([FromBody] IEnumerable<SimpleDataForHookTest> payload)
        {
            return await SendWebhookToSubscribers(payload);
        }

        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] SubscriptionRequest request)
        {
            await _hubContext.Clients.All.SendAsync("Subscribe", request.Url);
            return Ok(new { status = "subscribed" });
        }

        [HttpPost("unsubscribe")]
        public async Task<IActionResult> Unsubscribe([FromBody] SubscriptionRequest request)
        {
            await _hubContext.Clients.All.SendAsync("Unsubscribe");
            return Ok(new { status = "unsubscribed" });
        }

        // Sends the webhook payload to all subscribers
        private async Task<IActionResult> SendWebhookToSubscribers(IEnumerable<SimpleDataForHookTest> payload)
        {
            try
            {
                var subscribers = UpdateHub.GetSubscribers();
                var client = _httpClientFactory.CreateClient();

                foreach (var subscriber in subscribers)
                {
                    // Send the user data, including age
                    var response = await client.PostAsJsonAsync(subscriber, payload);
                    response.EnsureSuccessStatusCode();
                }

                return Ok(new { status = "webhook sent" });
            }
            catch (Exception ex)
            {
                // Return an error if the webhook failed
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
