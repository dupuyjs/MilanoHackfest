using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Samples;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
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

                bool isUploaded = await UploadToBlob(context, imageBuffer);

                Vision result = await MakeAnalysisRequest(imageBuffer, "d44d077e88e24f1ab5dbda8c0794455a");

                var entity = new VisionEntity() {
                    Value = result.description.captions.FirstOrDefault().text,
                    Type = "Description"
                };

                var topIntent = new Intent { Name = "intent.image", Score = 1.0 };
                topIntent.Entities.Add(entity);

                context.TopIntent = topIntent;
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


        public class VisionEntity : Entity
        {
            public VisionEntity()
            {

            }

            public string Type { get; set; }
            public string Value { get; set; }
        }

        private async Task<bool> UploadToBlob(IBotContext context, byte[] imgBuffer)
        {
            var message = context.Request.AsMessageActivity();
            var attachment = message.Attachments[0];
            var filename = attachment.Name;
            var filetype = attachment.ContentType;  


            CloudStorageAccount account = CloudStorageAccount.Parse(Startup.BlobConnectionString);
            CloudBlobClient blobClient = account.CreateCloudBlobClient();

            try
            {
                if (blobClient != null)
                {
                    CloudBlobContainer container = blobClient.GetContainerReference(Startup.BlobContainerName);

                    await container.CreateIfNotExistsAsync();

                    CloudBlockBlob blockBlob = container.GetBlockBlobReference($"{filename}");
                    blockBlob.Properties.ContentType = filetype;

                    using (var memoryStream = new System.IO.MemoryStream(imgBuffer))
                    {
                        await blockBlob.UploadFromStreamAsync(memoryStream);

                        return true;
                    }
                }

            }
            catch (Exception ex)
            {
                Trace.Write(ex, "BlobStorage - Uploadfile");             
            }
            return false;


        }

    }
}
