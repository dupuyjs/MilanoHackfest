using SJBot.Models;
using Microsoft.Bot.Builder;
using System.Collections.Generic;
using Microsoft.Bot.Schema;
using SJBot.Cards;
using AdaptiveCards;
using System.Linq;

namespace SJBot.Views
{
    public class WorkItemsView
    {
        public static void ShowWorkItems(IBotContext context, List<Workitem> workitems, bool lastOnly = false)
        {
            if ((workitems == null) || (workitems.Count == 0))
            {
                context.Reply("You have no workitems saved.");
                return;
            }

            List<Attachment> attachments = new List<Attachment>();

            if (lastOnly)
            {
                workitems = new List<Workitem>() { workitems.LastOrDefault() };
            }

            foreach (var item in workitems)
            {
                var card = new WorkItemCard(item);

                Attachment attachment = new Attachment()
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = card.GetCard()
                };

                attachments.Add(attachment);
            }
            var activity = MessageFactory.Carousel(attachments);
            context.Reply(activity);
        }
    }

}
