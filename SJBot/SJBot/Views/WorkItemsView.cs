using SJBot.Models;
using Microsoft.Bot.Builder;
using System.Collections.Generic;
using Microsoft.Bot.Schema;

namespace SJBot.Views
{
    public class WorkItemsView
    {
        public static void ShowWorkItems(IBotContext context, List<Workitem> workitems)
        {
            if ((workitems == null) || (workitems.Count == 0))
            {
                context.Reply("You have no alarms.");
                return;
            }

            List<Attachment> attachments = new List<Attachment>();
            foreach (var item in workitems)
            {
                HeroCard heroCard = new HeroCard()
                {
                    Title = $"WorkItem object: {item.Object}",
                    Subtitle = $"Customer Id {item.Customerid}",
                    Images = new List<CardImage>()
                            {
                                new CardImage() { Url = $"https://placeholdit.imgix.net/~text?txtsize=35&txt={item.Object}&w=500&h=260" }
                            }
                };

                AdaptiveCard

                attachments.Add(heroCard.ToAttachment());
            }
            var activity = MessageFactory.Carousel(attachments);
            context.Reply(activity);
        }
    }

}
