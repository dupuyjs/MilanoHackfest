using PromptlyBot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading.Tasks;
using SJBot.Models;
using SJBot.Views;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Samples;
using Microsoft.Extensions.Configuration;
using static SJBot.Middleware.AttachmentMiddleware;

namespace SJBot.Topics
{
    public class RootTopic : TopicsRoot
    {
        public IHttpContextAccessor Accessor { get; set; }

        public RootTopic(IBotContext context) : base(context)
        {
            // User state initialization should be done once in the welcome 
            //  new user feature. Placing it here until that feature is added.
            if (context.State.UserProperties[Constants.USER_STATE_WORKITEMS] == null)
            {
                context.State.UserProperties[Constants.USER_STATE_WORKITEMS] = new List<Workitem>();
            }

            this.SubTopics.Add(Constants.ADD_WORKITEM_TOPIC, () =>
            {
                var addWorkitemTopic = new AddWorkItemTopic();

                if (context.State.UserProperties["owner"] != null && addWorkitemTopic.State.Workitem != null)
                {
                    addWorkitemTopic.State.Workitem.Owner = context.State.UserProperties["owner"];
                }

                addWorkitemTopic.Set
                    .OnSuccess((ctx, workitem) =>
                    {
                        this.ClearActiveTopic();

                        ((List<Workitem>)ctx.State.UserProperties[Constants.USER_STATE_WORKITEMS]).Add(workitem);

                        WorkItemsView.ShowWorkItems(context, context.State.UserProperties[Constants.USER_STATE_WORKITEMS], Accessor, true);

                        SqlUtils sql = new SqlUtils(Startup.ConnectionString);
                        sql.CreateNewWorkItem(workitem);
                    })
                    .OnFailure((ctx, reason) =>
                    {
                        this.ClearActiveTopic();

                        context.Reply("Let's try something else.");

                        this.ShowDefaultMessage(ctx);
                    });

                return addWorkitemTopic;
            });
        }

        public override Task OnReceiveActivity(IBotContext context)
        {
            if ((context.Request.Type == ActivityTypes.Message) &&
                (!string.IsNullOrEmpty(context.Request.AsMessageActivity().Text) || context.Request.AsMessageActivity().Attachments != null))
            {
                var message = context.Request.AsMessageActivity();

                // If the user wants to change the topic of conversation...
                if (context.TopIntent != null && context.TopIntent.Score > 0.7)
                {
                    if (context.TopIntent.Name == "intent.image")
                    {
                        context.Reply(((VisionEntity)context.TopIntent.Entities[0]).Value);
                    }

                    if (context.TopIntent.Name == "intent.currentuser")
                    {
                        if (context.State.UserProperties["owner"] != null)
                        {
                            context.Reply($"Current user: {context.State.UserProperties["owner"]}");
                        }
                        else
                        {
                            context.Reply($"I've not recognized yet. What's your name?");                        
                        }
                        return Task.CompletedTask;
                    }

                    if (context.TopIntent.Name == "intent.workitem.add")
                    {
                        // Set the active topic and let the active topic handle this turn.
                        this.SetActiveTopic(Constants.ADD_WORKITEM_TOPIC)
                                .OnReceiveActivity(context);
                        return Task.CompletedTask;
                    }

                    if (context.TopIntent.Name == "intent.workitem.list")
                    {
                        this.ClearActiveTopic();

                        SqlUtils sql = new SqlUtils(Startup.ConnectionString);
                        var owner = context.State.UserProperties["owner"];

                        //WorkItemsView.ShowWorkItems(context, context.State.UserProperties[Constants.USER_STATE_WORKITEMS], Accessor);
                        WorkItemsView.ShowWorkItems(context, sql.GetWorkItems(owner), Accessor);

                        return Task.CompletedTask;
                    }

                    if (context.TopIntent.Name == "intent.help")
                    {
                        this.ClearActiveTopic();

                        this.ShowHelp(context);
                        return Task.CompletedTask;
                    }

                    if (context.TopIntent.Name == "intent.restart")
                    {
                        this.ClearActiveTopic();
                        context.State.ConversationProperties.Clear();
                        context.State.UserProperties.Clear();
                        //return Task.CompletedTask;
                    }
                }

                // If there is an active topic, let it handle this turn until it completes.
                if (HasActiveTopic)
                {
                    ActiveTopic.OnReceiveActivity(context);
                    return Task.CompletedTask;
                }

                ShowDefaultMessage(context);
            }

            return Task.CompletedTask;
        }

        private void ShowDefaultMessage(IBotContext context)
        {
            if (context.State.ConversationProperties["flagInit"] == null)
            {
                // Prompt user for name
                context.Reply("Hi. What's your name?");
                // Set flag to some non-null value
                context.State.ConversationProperties["flagInit"] = true;
            }
            else
            {
                if (context.State.UserProperties["owner"] == null)
                {
                    // Save user's name in context.State.UserProperties["name"]
                    var name = context.Request.AsMessageActivity().Text;
                    context.State.UserProperties["owner"] = name;
                    // Greet user
                    context.Reply($"Nice to meet you, {name}.");                   
                }

                //context.Reply("Choose an action: 'Add workitem', 'Show workitems', 'Help'.");
                context.Reply("How can I help you? Type 'help' to show actions available.");
            }
        }

        private void ShowHelp(IBotContext context)
        {
            var message = "Here's what I can do:\n\n";
            message += "To see your work items, you could say 'Show workitems'.\n\n";
            message += "To add a work item, you could say 'Add workitem'.\n\n";
            message += "To see this again, you could say 'Help'.";

            context.Reply(message);
        }
    }

}
