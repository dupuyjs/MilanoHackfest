using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Ai;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Builder.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SJBot.Middleware;
using System.Text.RegularExpressions;


namespace SJBot
{
    public class Startup
    {
        public static string ConnectionString { get; private set; }
        public static string BlobConnectionString { get; private set; }
        public static string BlobEndPoint { get; private set; }
        public static string BlobContainerName { get; private set; }


        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            ConnectionString = Configuration.GetSection("ConnectionStrings").GetSection("DefaultConnection").Value;
            BlobConnectionString = Configuration.GetSection("BlobConnection").GetSection("DefaultConnection").Value;
            BlobEndPoint = Configuration.GetSection("BlobConnection").GetSection("EndPoint").Value;
            BlobContainerName = Configuration.GetSection("BlobConnection").GetSection("ContainerName").Value;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(_ => Configuration);
            services.AddMvc();

            services.AddSingleton<BotFrameworkAdapter>(_ =>
            {
                return new BotFrameworkAdapter(Configuration)
                    .Use(new ConversationStateManagerMiddleware(new MemoryStorage()))
                    .Use(new UserStateManagerMiddleware(new MemoryStorage()))
                    .Use(new AttachmentMiddleware())
                    .Use(new LuisRecognizerMiddleware("c66fd498-6a8c-4e26-b7d9-5b51b640092a", "68f543767b3a49afae9da0f10491d5fb"));

            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IConfiguration>(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMvc();
        }
    }
}