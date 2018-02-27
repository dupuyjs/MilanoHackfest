using Microsoft.Bot.Builder;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Number;
using PromptlyBot;
using PromptlyBot.Validator;
using SJBot.Models;
using System.Threading.Tasks;
using System.Linq;


namespace SJBot.Topics
{
    public class AddWorkItemTopicState : ConversationTopicState
    {
        public Workitem Workitem = new Workitem();
    }

    public class AddWorkItemTopic : ConversationTopic<AddWorkItemTopicState, Workitem>
    {
        public AddWorkItemTopic() : base()
        {

            this.SubTopics.Add(Constants.CUSTOMERID_PROMPT, () =>
            {
                var customeridPrompt = new Prompt<int>();

                customeridPrompt.Set
                    .OnPrompt((context, lastTurnReason) =>
                    {
                        if ((lastTurnReason != null) && (lastTurnReason == Constants.INT_ERROR))
                        {
                            context.Reply("Sorry, customer id must be a number.")
                            .Reply("Let's try again.");
                        }

                        context.Reply("Enter the customer id.");
                    })
                    .Validator(new CustomerIdValidator())
                    .MaxTurns(2)
                    .OnSuccess((context, value) =>
                    {
                        this.ClearActiveTopic();

                        this.State.Workitem.Customerid = value;

                        this.OnReceiveActivity(context);
                    })
                    .OnFailure((context, reason) =>
                    {
                        this.ClearActiveTopic();

                        if ((reason != null) && (reason == "toomanyattempts"))
                        {
                            context.Reply("I'm sorry I'm having issues understanding you.");
                        }

                        this.OnFailure(context, reason);
                    });

                return customeridPrompt;
            });

            this.SubTopics.Add(Constants.HOURS_PROMPT, () =>
            {
                var hoursPrompt = new Prompt<int>();

                hoursPrompt.Set
                    .OnPrompt((context, lastTurnReason) =>
                    {
                        context.Reply("Enter hours worked.");
                    })
                    .Validator(new HoursValidator())
                    .MaxTurns(2)
                    .OnSuccess((context, value) =>
                    {
                        this.ClearActiveTopic();

                        this.State.Workitem.Hours = value;

                        this.OnReceiveActivity(context);
                    })
                    .OnFailure((context, reason) =>
                    {
                        this.ClearActiveTopic();

                        if ((reason != null) && (reason == "toomanyattempts"))
                        {
                            context.Reply("I'm sorry I'm having issues understanding you.");
                        }

                        this.OnFailure(context, reason);
                    });

                return hoursPrompt;
            });

            this.SubTopics.Add(Constants.OBJECT_PROMPT, () =>
            {
                var hoursPrompt = new Prompt<string>();

                hoursPrompt.Set
                    .OnPrompt((context, lastTurnReason) =>
                    {
                        context.Reply("Enter workitem's object.");
                    })
                    .Validator(new ObjectValidator())
                    .MaxTurns(2)
                    .OnSuccess((context, value) =>
                    {
                        this.ClearActiveTopic();

                        this.State.Workitem.Object = value;

                        this.OnReceiveActivity(context);
                    })
                    .OnFailure((context, reason) =>
                    {
                        this.ClearActiveTopic();

                        if ((reason != null) && (reason == "toomanyattempts"))
                        {
                            context.Reply("I'm sorry I'm having issues understanding you.");
                        }

                        this.OnFailure(context, reason);
                    });

                return hoursPrompt;
            });

            this.SubTopics.Add(Constants.DESCRIPTION_PROMPT, () =>
            {
                var hoursPrompt = new Prompt<string>();

                hoursPrompt.Set
                    .OnPrompt((context, lastTurnReason) =>
                    {
                        context.Reply("Enter hours worked.");
                    })
                    .Validator(new DescriptionValidator())
                    .MaxTurns(2)
                    .OnSuccess((context, value) =>
                    {
                        this.ClearActiveTopic();

                        this.State.Workitem.Description = value;

                        this.OnReceiveActivity(context);
                    })
                    .OnFailure((context, reason) =>
                    {
                        this.ClearActiveTopic();

                        if ((reason != null) && (reason == "toomanyattempts"))
                        {
                            context.Reply("I'm sorry I'm having issues understanding you.");
                        }

                        this.OnFailure(context, reason);
                    });

                return hoursPrompt;
            });

        }

        public override Task OnReceiveActivity(IBotContext context)
        {
            if (HasActiveTopic)
            {
                ActiveTopic.OnReceiveActivity(context);
                return Task.CompletedTask;
            }

            if (this.State.Workitem.Customerid == null)
            {
                this.SetActiveTopic(Constants.CUSTOMERID_PROMPT);
                this.ActiveTopic.OnReceiveActivity(context);
                return Task.CompletedTask;
            }

            if (this.State.Workitem.Hours == null)
            {
                this.SetActiveTopic(Constants.HOURS_PROMPT);
                this.ActiveTopic.OnReceiveActivity(context);
                return Task.CompletedTask;
            }

            this.OnSuccess(context, this.State.Workitem);

            return Task.CompletedTask;
        }
    }

    public class CustomerIdValidator : Validator<int>
    {
        public override ValidatorResult<int> Validate(IBotContext context)
        {
            // Recognize number
            NumberModel numberModel = (NumberModel)NumberRecognizer.Instance.GetNumberModel(Culture.English);
            var result = numberModel.Parse(context.Request.AsMessageActivity().Text);

            if (result.Count > 0 && int.TryParse(result[0].Resolution.Values.FirstOrDefault().ToString(), out int n))
            {                 
                return new ValidatorResult<int>
                {
                    Value = n
                };
            }
            else
            {
                return new ValidatorResult<int>
                {
                    Reason = Constants.INT_ERROR
                };
            }   
        }
    }

    public class HoursValidator : Validator<int>
    {
        public override ValidatorResult<int> Validate(IBotContext context)
        {
            // Recognize number
            NumberModel numberModel = (NumberModel)NumberRecognizer.Instance.GetNumberModel(Culture.English);
            var result = numberModel.Parse(context.Request.AsMessageActivity().Text);

            if (result.Count > 0 && int.TryParse(result[0].Resolution.Values.FirstOrDefault().ToString(), out int n))
            {
                return new ValidatorResult<int>
                {
                    Value = n
                };
            }
            else
            {
                return new ValidatorResult<int>
                {
                    Reason = Constants.INT_ERROR
                };
            }
        }
    }

    public class ObjectValidator : Validator<string>
    {
        public override ValidatorResult<string> Validate(IBotContext context)
        {
            return new ValidatorResult<string>
            {
                Value = context.Request.AsMessageActivity().Text
            };
        }
    }

    public class DescriptionValidator : Validator<string>
    {
        public override ValidatorResult<string> Validate(IBotContext context)
        {
            return new ValidatorResult<string>
            {
                Value = context.Request.AsMessageActivity().Text
            };
        }
    }

}

