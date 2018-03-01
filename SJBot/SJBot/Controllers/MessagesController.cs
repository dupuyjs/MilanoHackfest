using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Number;
using SJBot.Models;
using SJBot.Topics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Bot.Samples
{
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        BotFrameworkAdapter adapter = null;
        IHttpContextAccessor httpContextAccessor = null;

        public MessagesController(BotFrameworkAdapter adapter, IHttpContextAccessor httpContextAccessor)
        {
            this.adapter = adapter;
            this.httpContextAccessor = httpContextAccessor;
        }

        public Task OnReceiveActivity(IBotContext context)
        {

            if (context.Request.Type == ActivityTypes.Message)
            {
                var rootTopic = new RootTopic(context);
                rootTopic.Accessor = httpContextAccessor;
                rootTopic.OnReceiveActivity(context);
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