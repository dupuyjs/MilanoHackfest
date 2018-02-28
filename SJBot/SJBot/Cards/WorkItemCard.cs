using AdaptiveCards;
using SJBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SJBot.Cards
{
    public class WorkItemCard
    {
        private readonly Workitem _workitem;

        public WorkItemCard(Workitem workitem)
        {
            this._workitem = workitem;
        }

        public AdaptiveCard GetCard()
        {
            AdaptiveCard card = new AdaptiveCard();

            card.Body.Add(new TextBlock()
            {
                Text = _workitem?.Object,
                Size = TextSize.Large,
                Weight = TextWeight.Bolder
            });

            // Customer
            card.Body.Add(new TextBlock()
            {
                Text = $"Customer: { _workitem?.Customer}"
            });

            // Duration
            card.Body.Add(new TextBlock()
            {
                Text = $"Date: {_workitem?.Date.Value.ToShortDateString()} - Hours: {_workitem?.Hours}"
            });

            card.Body.Add(new Image()
            {
                Url = "http://localhost:59929/images/workitem.png",
                Style = ImageStyle.Normal
            });

            // Get Desciption
            card.Actions.Add(new AdaptiveCards.SubmitAction()
            {
                Data = "description",
                Title = "Get Description"
            });

            return card;
        }
    }
}
