using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Number;
using SJBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Bot.Samples
{
    [Route("api/[controller]")]
    public class _MessagesController : Controller
    {
        BotFrameworkAdapter adapter = null;

        public _MessagesController(BotFrameworkAdapter adapter)
        {
            this.adapter = adapter;
        }

        public Task AddWorkItem(IBotContext context)
        {
            //switch
            var message = context.Request.AsMessageActivity();

            if (context.State.ConversationProperties["currentState"] == null)
            {
                context.Reply("Enter customerid:");
                context.State.ConversationProperties["currentState"] = "customerId";

                return Task.CompletedTask;
            }

            switch (context.State.ConversationProperties["currentState"])
            {
                case "customerId":
                    context.State.ConversationProperties["customerId"] = message.Text;
                    context.Reply("Enter description:");
                    context.State.ConversationProperties["currentState"] = "description";
                    break;

                case "description":
                    context.State.ConversationProperties["description"] = message.Text;
                    context.Reply("Enter work hours:");

                    context.State.ConversationProperties["currentState"] = "hours";                 
                    break;

                case "hours":
                    context.State.ConversationProperties["hours"] = message.Text;
                   
                    // Recognize number
                    NumberModel numberModel = (NumberModel)NumberRecognizer.Instance.GetNumberModel(Culture.English);
                    var result = numberModel.Parse(context.Request.AsMessageActivity().Text);

                    if (result.Count > 0 && long.TryParse(result[0].Resolution.Values.FirstOrDefault().ToString(), out long n))
                    {
                        context.Reply($"You entered: {n}");
                        context.Reply("Enter attachment:");
                        context.State.ConversationProperties["currentState"] = "attachment";
                    }
                    else
                    {
                        context.Reply("Invalid number. Enter an integer.");
                    }
                    break;

                default:
                    // Once you completed your steps
                    context.State.ConversationProperties["currentIntent"] = null;
                    break;
            }

            return Task.CompletedTask;
        }

        public Task OnReceiveActivity(IBotContext context)
        {
            if (context.Request.Type is ActivityTypes.Message)
            {
                switch (context.State.ConversationProperties["currentIntent"])
                {
                    case "addWorkItems":
                        AddWorkItem(context);
                        break;
                    default:
                        break;
                }

                switch (context.TopIntent?.Name)
                {
                    case "showWorkItems":
                        context.Reply("Here is the list.");
                        // TODO - Query the items
                        break;
                    case "addWorkItems":
                        context.Reply("Add workitems element.");
                        context.State.ConversationProperties["currentIntent"] = "addWorkItems";
                        AddWorkItem(context);
                        break;
                    default:
                   
                        break;
                }

                //// Use context.State.ConversationProperties["flag"] as a flag for setting the user's name
                //if (context.State.ConversationProperties["flag"] == null)
                //{
                //    // Prompt user for name
                //    context.Reply("Hi. What's your name?");
                //    // Set flag to some non-null value
                //    context.State.ConversationProperties["flag"] = true;
                //}
                //else
                //{
                //    if (context.State.UserProperties["name"] == null)
                //    {
                //        // Save user's name in context.State.UserProperties["name"]
                //        var name = context.Request.AsMessageActivity().Text;
                //        context.State.UserProperties["name"] = name;
                //        // Greet user
                //        context.Reply($"Nice to meet you, {name}.");
                //    }


                //    switch (context.TopIntent?.Name)
                //    {
                //        case "showWorkItems":
                //            context.Reply("Here is the list.");
                //            context.State.ConversationProperties["flagRead"] = true;
                //            break;
                //        case "addWorkItems":
                //            context.Reply("Add workiitems element.");
                //            context.State.ConversationProperties["flagAdd"] = true;
                //            break;
                //        default:
                //            break;


                //    }

                //    if (context.State.ConversationProperties["flagRead"] != null)
                //    {
                //        List<Attachment> attachments = new List<Attachment>();
                //        for (int i = 0; i < 3; i++)
                //        {
                //            HeroCard heroCard = new HeroCard()
                //            {
                //                Title = $"WorkItem titte {i}",
                //                Subtitle = $"Activity {i}",
                //                Images = new List<CardImage>()
                //                {
                //                    new CardImage() { Url = $"https://placeholdit.imgix.net/~text?txtsize=35&txt=Item+{i}&w=500&h=260" }
                //                }
                //            };

                //            attachments.Add(heroCard.ToAttachment());
                //        }

                //        var activity = MessageFactory.Carousel(attachments);
                //        context.Reply(activity);
                //        context.State.ConversationProperties["flagRead"] = null;
                //    }

                //    if (context.State.ConversationProperties["flagAdd"] != null)
                //    {

                //        if (context.State.ConversationProperties["Add_customerid"] == null &&
                //            context.State.ConversationProperties["Add_description"] == null &&
                //            context.State.ConversationProperties["Add_hours"] == null)
                //        {
                //            if (context.State.ConversationProperties["flagCustomeid"] == null)
                //            {
                //                context.Reply("Enter customerid:");
                //                context.State.ConversationProperties["flagCustomeid"] = true;
                //            }
                //            else
                //            {
                //                var customerid = context.Request.AsMessageActivity().Text;
                //                context.State.ConversationProperties["Add_customerid"] = customerid;
                //            }
                //        }

                //        if (context.State.ConversationProperties["Add_customerid"] != null &&
                //            context.State.ConversationProperties["Add_description"] == null &&
                //            context.State.ConversationProperties["Add_hours"] == null)
                //        {
                //            context.Reply("Enter description:");
                //            var description = context.Request.AsMessageActivity().Text;
                //            context.State.ConversationProperties["Add_description"] = description;
                //        }

                //        if (context.State.ConversationProperties["Add_customerid"] != null &&
                //            context.State.ConversationProperties["Add_description"] != null &&
                //            context.State.ConversationProperties["Add_hours"] == null)
                //        {
                //            context.Reply("Enter hours you spent:");
                //            var hours = context.Request.AsMessageActivity().Text;
                //            context.State.ConversationProperties["Add_hours"] = hours;
                //        }

                //        if(context.State.ConversationProperties["Add_hours"] != null &&
                //            context.State.ConversationProperties["Add_description"] != null&&
                //            context.State.UserProperties["name"] != null &&
                //            context.State.ConversationProperties["Add_customerid"] != null)
                //        {
                //            Workitem item = new Workitem();
                //            item.Hours = context.State.ConversationProperties["Add_hours"];
                //            item.Description = context.State.ConversationProperties["Add_description"];
                //            item.Owner = context.State.UserProperties["name"] ;
                //            item.Customerid = context.State.ConversationProperties["Add_customerid"];

                //            context.Reply($"Workitem added - Description:{item.Description}, Owner:{item.Owner}, Customerid:{item.Customerid}, hours:{item.Hours}");

                //            context.State.UserProperties["Add_hours"] = null;
                //            context.State.UserProperties["Add_description"] = null;
                //            context.State.UserProperties["Add_customerid"] = null;
                //        }

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