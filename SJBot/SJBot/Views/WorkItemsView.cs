using SJBot.Models;
using Microsoft.Bot.Builder;
using System.Collections.Generic;
using Microsoft.Bot.Schema;
using SJBot.Cards;
using AdaptiveCards;

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
                var card = new WorkItemCard(item);

                Attachment attachment = new Attachment()
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = card.GetCard()
                };

                //HeroCard heroCard = new HeroCard()
                //{
                //    Title = $"WorkItem object: {item.Object}",
                //    Subtitle = $"Customer Id {item.Customerid}",
                //    Images = new List<CardImage>()
                //            {
                //                new CardImage() { Url = $"https://placeholdit.imgix.net/~text?txtsize=35&txt={item.Object}&w=500&h=260" }
                //            }
                //};

                //TODO
                //AdaptiveCard

                attachments.Add(attachment);
            }
            var activity = MessageFactory.Carousel(attachments);
            context.Reply(activity);
        }
    }

}
