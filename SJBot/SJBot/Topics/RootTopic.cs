using PromptlyBot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading.Tasks;
using SJBot.Models;
using SJBot.Views;

namespace SJBot.Topics
{
    public class RootTopic : TopicsRoot
    {


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

                        context.Reply($"Added workitem for customer '{ workitem.Customerid }' set by '{ workitem.Owner }'.");
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
            if ((context.Request.Type == ActivityTypes.Message) && (context.Request.AsMessageActivity().Text.Length > 0))
            {
                var message = context.Request.AsMessageActivity();

                // If the user wants to change the topic of conversation...
                if (message.Text.ToLowerInvariant() == "add workitem")
                {
                    // Set the active topic and let the active topic handle this turn.
                    this.SetActiveTopic(Constants.ADD_WORKITEM_TOPIC)
                            .OnReceiveActivity(context);
                    return Task.CompletedTask;
                }

                if (message.Text.ToLowerInvariant() == "show workitems")
                {
                    this.ClearActiveTopic();

                    WorkItemsView.ShowWorkItems(context, context.State.UserProperties[Constants.USER_STATE_WORKITEMS]);
                    return Task.CompletedTask;
                }

                if (message.Text.ToLowerInvariant() == "help")
                {
                    this.ClearActiveTopic();

                    this.ShowHelp(context);
                    return Task.CompletedTask;
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

                context.Reply("Choose an action: 'Add workitem', 'Show workitems', 'Help'.");

            }
        }

        private void ShowHelp(IBotContext context)
        {
            var message = "Here's what I can do:\n\n";
            message += "To see your work items, say 'Show workitems'.\n\n";
            message += "To add a work item, say 'Add workitem'.\n\n";
            message += "To see this again, say 'Help'.";

            context.Reply(message);
        }
    }

}
