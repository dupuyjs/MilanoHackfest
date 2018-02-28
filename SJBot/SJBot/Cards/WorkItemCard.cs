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
        private readonly Workitem _workItem;

        public WorkItemCard(Workitem workitem)
        {
            this._workItem = workitem;
        }

        public AdaptiveCard GetCard()
        {
            AdaptiveCard card = new AdaptiveCard();

            card.Body.Add(new TextBlock()
            {
                Text = $"#{_workItem?.Customerid} - {_workItem?.Object}",
                Size = TextSize.Large,
                Weight = TextWeight.Bolder
            });

            // Owner
            card.Body.Add(new TextBlock()
            {
                Text = _workItem?.Owner?.ToString()
            });

            // Duration
            card.Body.Add(new TextBlock()
            {
                Text = _workItem?.Hours?.ToString()
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
