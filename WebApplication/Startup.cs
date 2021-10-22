using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Dapr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebApplication
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDaprClient(builder =>
            {
                builder.UseJsonSerializationOptions(GetJsonSerializerOptions());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseCloudEvents();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapSubscribeHandler();

                endpoints.MapPost("matched-route", RouteMatchedHandler).WithMetadata(
                    new TopicAttribute(
                        "pubsub", "topic1",  "event.type == \"event.v1\"", 1
                    )
                );
                endpoints.MapPost("default-route", RouteDefaultHandler).WithMetadata(
                    new TopicAttribute(
                        "pubsub", "topic1"
                    )
                );
            });
            
            async Task RouteMatchedHandler(HttpContext context)
            {
                var reader = new StreamReader(context.Request.Body);
                string message = await reader.ReadToEndAsync();
                ILogger<Startup> logger = context.RequestServices.GetRequiredService<ILogger<Startup>>();
                logger.LogInformation("MATCHED HANDLER INCOMING MESSAGE: {Message}", message);
            }
            
            async Task RouteDefaultHandler(HttpContext context)
            {
                var reader = new StreamReader(context.Request.Body);
                string message = await reader.ReadToEndAsync();
                ILogger<Startup> logger = context.RequestServices.GetRequiredService<ILogger<Startup>>();
                logger.LogInformation("DEFAULT HANDLER INCOMING MESSAGE: {Message}", message);
            }
        }

        private static JsonSerializerOptions GetJsonSerializerOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
        }
    }
}