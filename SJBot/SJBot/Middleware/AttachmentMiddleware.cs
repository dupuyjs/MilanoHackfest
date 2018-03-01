using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Middleware;
using Newtonsoft.Json;
using SJBot.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SJBot.Middleware
{
    public class AttachmentMiddleware : IReceiveActivity
    {
        public async Task ReceiveActivity(IBotContext context, MiddlewareSet.NextDelegate next)
        {
            if (HasImageAttachment(context))
            {
                byte[] imageBuffer = await GetImageFromMessageAsync(context);

                Vision result = await MakeAnalysisRequest(imageBuffer, "d44d077e88e24f1ab5dbda8c0794455a");

                IList<Entity> entities = new List<Entity>();
                entities.Add(new Entity() {
                    GroupName = result.description.captions.FirstOrDefault().text,
                    Score = result.description.captions.FirstOrDefault().confidence
                });

                context.TopIntent = new Intent { Name = "intent.image", Score = 1.0};
            }

            await next().ConfigureAwait(false);
        }

        public bool HasImageAttachment(IBotContext context)
        {
            var message = context.Request.AsMessageActivity();

            if (message?.Attachments != null)
            {
                return message.Attachments.Count > 0 &&
                    message.Attachments[0].ContentType.Contains("image");
            }

            return false;
        }

        public bool CheckRequiresToken(IBotContext context)
        {
            var message = context.Request.AsMessageActivity();

            if (!string.IsNullOrEmpty(context?.Request?.ChannelId))
            {
                return message.ChannelId == "skype" || message.ChannelId == "msteams";
            }

            return false;
        }

        public async Task<byte[]> GetImageFromMessageAsync(IBotContext context)
        {
            byte[] imageBuffer = null;

            if (HasImageAttachment(context))
            {
                var message = context.Request.AsMessageActivity();
                var attachment = message.Attachments[0];
                var url = attachment.ContentUrl;

                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        using (var response = await client.GetAsync(url))
                        {
                            response.EnsureSuccessStatusCode();

                            imageBuffer = await response.Content.ReadAsByteArrayAsync();
                            return imageBuffer;
                        }
                    };
                }
                catch (Exception ex)
                {
                    Trace.Write(ex.Message);
                }
            }

            return imageBuffer;
        }

        static async Task<Vision> MakeAnalysisRequest(byte[] imageBuffer, string subscriptionKey)
        {
            const string uriBase = "https://westeurope.api.cognitive.microsoft.com/vision/v1.0/analyze";
            Vision visionResult = null;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Request headers.
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

                    // Request parameters. A third optional parameter is "details".
                    string requestParameters = "visualFeatures=Description";

                    // Assemble the URI for the REST API Call.
                    string uri = uriBase + "?" + requestParameters;

                    using (ByteArrayContent content = new ByteArrayContent(imageBuffer))
                    {
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                        // Execute the REST API call.
                        var response = await client.PostAsync(uri, content);

                        response.EnsureSuccessStatusCode();

                        // Get the JSON response.
                        string json = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<Vision>(json);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.Write(ex.Message);
            }

            return visionResult;
        }
    }
}
