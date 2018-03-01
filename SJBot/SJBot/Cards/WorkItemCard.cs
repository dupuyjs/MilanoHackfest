using AdaptiveCards;
using Microsoft.AspNetCore.Http;
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

        public AdaptiveCard GetCard(IHttpContextAccessor accessor)
        {
            AdaptiveCard card = new AdaptiveCard();

            var columnSet = new ColumnSet();
            card.Body.Add(columnSet);

            var firstColumn = new Column();
            columnSet.Columns.Add(firstColumn);
            firstColumn.Size = "auto";

            var secondColumn = new Column();
            columnSet.Columns.Add(secondColumn);
            secondColumn.Size = "stretch";

            firstColumn.Items.Add(new TextBlock()
            {
                Text = _workitem?.Object,
                Size = TextSize.Large,
                Weight = TextWeight.Bolder
            });

            // Customer
            firstColumn.Items.Add(new TextBlock()
            {
                Text = $"Customer: { _workitem?.Customer}"
            });

            // Duration
            firstColumn.Items.Add(new TextBlock()
            {
                Text = $"Date: {_workitem?.Date.Value.ToShortDateString()} - Hours: {_workitem?.Hours}"
            });

            secondColumn.Items.Add(new Image()
            {
                Url = $"http://{accessor.HttpContext.Request.Host}/images/workitem.png",
                Style = ImageStyle.Normal,
                HorizontalAlignment = HorizontalAlignment.Right
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
