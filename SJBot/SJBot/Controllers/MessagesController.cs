using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Bot.Samples
{
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        BotFrameworkAdapter adapter = null;

        public MessagesController(BotFrameworkAdapter adapter)
        {
            this.adapter = adapter;
        }

        public Task OnReceiveActivity(IBotContext context)
        {
            if (context.Request.Type is ActivityTypes.Message)
            {
                context.Reply($"Hello world.");

                // Use context.State.ConversationProperties["flag"] as a flag for setting the user's name
                if (context.State.ConversationProperties["flag"] == null)
                {
                    // Prompt user for name
                    context.Reply("Hi. What's your name?");
                    // Set flag to some non-null value
                    context.State.ConversationProperties["flag"] = true;
                }
                else
                {
                    // Save user's name in context.State.UserProperties["name"]
                    var name = context.Request.AsMessageActivity().Text;
                    context.State.UserProperties["name"] = name;
                    // Greet user
                    context.Reply($"Nice to meet you, {name}.");
                    // Reset flag to null
                    context.State.ConversationProperties["flag"] = null;
                }

            }

            return Task.CompletedTask;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Activity activity)
        {
            try
            {
                await adapter.ProcessActivty(this.Request.Headers["Authorization"].FirstOrDefault(), activity, OnReceiveActivity);
                return this.Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return this.Unauthorized();
            }
            catch (InvalidOperationException e)
            {
                return this.NotFound(e.Message);
            }
        }
    }
}