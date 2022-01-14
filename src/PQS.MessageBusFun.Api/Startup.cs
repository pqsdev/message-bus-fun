
namespace PQS.MessageBusFun.Api
{
    using MassTransit;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Server.Kestrel.Core;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using PQS.MessageBusFun.Api.Components;
    using PQS.MessageBusFun.Messaging.Contracts;
    using Serilog;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class Startup
    {


        static bool? _isRunningInContainer;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public static bool IsRunningInContainer =>
            _isRunningInContainer ??= bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), out var inDocker) && inDocker;


        public void ConfigureServices(IServiceCollection services)
        {


            services.Configure<KestrelServerOptions>(Configuration.GetSection("Kestrel"));
            services.Configure<RabbitMQConfig>(Configuration.GetSection(RabbitMQConfig.SECTION_NAME));


            services.AddControllers();



            services.AddMassTransit(x =>
            {
                x.AddDelayedMessageScheduler();

                x.AddConsumersFromNamespaceContaining<JobDoneConsumer>();
                x.AddRequestClient<IDoJob>();
                x.AddRequestClient<IGreetingCmd>();
                x.SetKebabCaseEndpointNameFormatter();


                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitConfig = context.GetRequiredService<IOptions<RabbitMQConfig>>().Value;

                    // TODO MEJORAR ESTO APRA TENER MAS OPCIONES VER QUE ONDA MassTransit.RabbitMqTransport.RabbitMqHostSettings
                    cfg.Host(rabbitConfig.Host, h =>
                    {
                        h.Username(rabbitConfig.Username);
                        h.Password(rabbitConfig.Password);
                    });
                    cfg.UseDelayedMessageScheduler();
                });
            });
            services.AddMassTransitHostedService();

            services.AddOpenApiDocument(cfg => cfg.PostProcess = d => d.Info.Title = "Convert Video Service");
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSerilogRequestLogging();

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseRouting();

            app.UseAuthorization();

            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("ready"),
                    ResponseWriter = HealthCheckResponseWriter
                });

                endpoints.MapHealthChecks("/health/live", new HealthCheckOptions { ResponseWriter = HealthCheckResponseWriter });

                endpoints.MapControllers();
            });
        }

        static Task HealthCheckResponseWriter(HttpContext context, HealthReport result)
        {
            var json = new JObject(
                new JProperty("status", result.Status.ToString()),
                new JProperty("results", new JObject(result.Entries.Select(entry => new JProperty(entry.Key, new JObject(
                    new JProperty("status", entry.Value.Status.ToString()),
                    new JProperty("description", entry.Value.Description),
                    new JProperty("data", JObject.FromObject(entry.Value.Data))))))));

            context.Response.ContentType = "application/json";

            return context.Response.WriteAsync(json.ToString(Formatting.Indented));
        }




    }
}
