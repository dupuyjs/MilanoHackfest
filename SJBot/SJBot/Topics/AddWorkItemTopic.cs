using Microsoft.Bot.Builder;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Number;
using PromptlyBot;
using PromptlyBot.Validator;
using SJBot.Models;
using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.Recognizers.Text.DateTime;
using System.Globalization;

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

            this.SubTopics.Add(Constants.CUSTOMER_PROMPT, () =>
            {
                var customerPrompt = new Prompt<string>();

                customerPrompt.Set
                    .OnPrompt((context, lastTurnReason) =>
                    {
                        context.Reply("Enter the customer:");
                    })
                    .Validator(new CustomerValidator())
                    .MaxTurns(2)
                    .OnSuccess((context, value) =>
                    {
                        this.ClearActiveTopic();

                        this.State.Workitem.Customer = value;

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

                return customerPrompt;
            });

            this.SubTopics.Add(Constants.OBJECT_PROMPT, () =>
            {
                var objectPrompt = new Prompt<string>();

                objectPrompt.Set
                    .OnPrompt((context, lastTurnReason) =>
                    {
                        context.Reply("Enter workitem's object:");
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

                return objectPrompt;
            });

            this.SubTopics.Add(Constants.DATE_PROMPT, () =>
            {
                var datePrompt = new Prompt<DateTime>();

                datePrompt.Set
                    .OnPrompt((context, lastTurnReason) =>
                    {
                        if ((lastTurnReason != null) && (lastTurnReason == Constants.DATE_ERROR))
                        {
                            context.Reply("Sorry, wrong date format. \n\n Try again.");
                        }
                        context.Reply("Enter workitem's date (YYYY-MM-DD):");
                    })
                    .Validator(new DateValidator())
                    .MaxTurns(2)
                    .OnSuccess((context, value) =>
                    {
                        this.ClearActiveTopic();

                        this.State.Workitem.Date = value;

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

                return datePrompt;
            });

            this.SubTopics.Add(Constants.HOURS_PROMPT, () =>
            {
                var hoursPrompt = new Prompt<int>();

                hoursPrompt.Set
                    .OnPrompt((context, lastTurnReason) =>
                    {
                        if ((lastTurnReason != null) && (lastTurnReason == Constants.INT_ERROR))
                        {
                            context.Reply("Sorry, hours worked must be typed as a number. \n\n Try again.");
                        }
                        context.Reply("Enter hours worked:");
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
         
            this.SubTopics.Add(Constants.DESCRIPTION_PROMPT, () =>
            {
                var hoursPrompt = new Prompt<string>();

                hoursPrompt.Set
                    .OnPrompt((context, lastTurnReason) =>
                    {
                        context.Reply("Provide a description of your activity.");
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

            if (this.State.Workitem.Customer == null)
            {
                this.SetActiveTopic(Constants.CUSTOMER_PROMPT);
                this.ActiveTopic.OnReceiveActivity(context);
                return Task.CompletedTask;
            }

            if (this.State.Workitem.Object == null)
            {
                this.SetActiveTopic(Constants.OBJECT_PROMPT);
                this.ActiveTopic.OnReceiveActivity(context);
                return Task.CompletedTask;
            }

            if (this.State.Workitem.Date == null)
            {
                this.SetActiveTopic(Constants.DATE_PROMPT);
                this.ActiveTopic.OnReceiveActivity(context);
                return Task.CompletedTask;
            }

            if (this.State.Workitem.Hours == null)
            {
                this.SetActiveTopic(Constants.HOURS_PROMPT);
                this.ActiveTopic.OnReceiveActivity(context);
                return Task.CompletedTask;
            }            

            if (this.State.Workitem.Description == null)
            {
                this.SetActiveTopic(Constants.DESCRIPTION_PROMPT);
                this.ActiveTopic.OnReceiveActivity(context);
                return Task.CompletedTask;
            }

            this.OnSuccess(context, this.State.Workitem);

            return Task.CompletedTask;
        }
    }

    public class CustomerValidator : Validator<string>
    {
        public override ValidatorResult<string> Validate(IBotContext context)
        {
            return new ValidatorResult<string>
            {
                Value = context.Request.AsMessageActivity().Text
            };
        }
    }

    public class DateValidator : Validator<DateTime>
    {
        public override ValidatorResult<DateTime> Validate(IBotContext context)
        {
            // Recognize Date
            DateTimeRecognizer dateRecognizer = DateTimeRecognizer.GetInstance(DateTimeOptions.None);
            DateTimeModel dateModel = dateRecognizer.GetDateTimeModel("en-US");
            var result = dateModel.Parse(context.Request.AsMessageActivity().Text);

            if (result.Count > 0 && !string.IsNullOrEmpty(result[0].Text))
            {           
                try
                {
                    return new ValidatorResult<DateTime>
                    {
                        Value = DateTime.ParseExact(result[0].Text, "yyyy-MM-dd", CultureInfo.CurrentCulture)
                    };
                }
                catch (Exception e)
                {
                    return new ValidatorResult<DateTime>
                    {
                        Reason = Constants.DATE_ERROR
                    };
                }
            }
            else
            {
                return new ValidatorResult<DateTime>
                {
                    Reason = Constants.DATE_ERROR
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

