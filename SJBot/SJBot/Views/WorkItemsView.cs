using SJBot.Models;
using Microsoft.Bot.Builder;
using System.Collections.Generic;
using Microsoft.Bot.Schema;
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
                HeroCard heroCard = new HeroCard()
                {
                    Title = $"Object: {item.Object} - Customer: {item.Customer}",
                    Subtitle = $"Date: {item.Date.Value.ToShortDateString()} - Hours: {item.Hours}",
                    Text = $"Description: {item.Description}",


                    //Images = new List<CardImage>()
                    //{
                    //    new CardImage() { Url = $"https://placeholdit.imgix.net/~text?txtsize=35&txt={item.Description}&w=500&h=260" }
                    //}

                };

                //TODO
                //AdaptiveCard

                attachments.Add(heroCard.ToAttachment());
            }

            var activity = MessageFactory.Carousel(attachments);
            context.Reply(activity);
        }
    }

}
