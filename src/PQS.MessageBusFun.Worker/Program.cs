namespace PQS.MessageBusFun.Worker
{
    using MassTransit;
    using MassTransit.Definition;
    using MassTransit.EntityFrameworkCoreIntegration.JobService;
    using MassTransit.JobService.Components.StateMachines;
    using MassTransit.JobService.Configuration;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;
    using PQS.MessageBusFun.Messaging.Components;
    using PQS.MessageBusFun.Messaging.Contracts;
    using Serilog;
    using System;
    using System.Reflection;

    public class Program
    {
        static bool? _isRunningInContainer;
        public static bool IsRunningInContainer =>
            _isRunningInContainer ??= bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), out var inDocker) && inDocker;

        public static void Main(string[] args)
        {
            try
            {
                Serilog.Debugging.SelfLog.Enable(Console.Error);
                Log.Information("Building host..");
                var host = CreateHostBuilder(args).Build();

                using (var scope = host.Services.CreateScope())
                {
                    Log.Information("Running db migrations..");
                    var db = scope.ServiceProvider.GetRequiredService<JobServiceSagaDbContext>();
                    db.Database.Migrate();
                    Log.Information("Migration OK");
                }
                Log.Information("Building host OK");
                Log.Information("Running host host...");
                host.Run();

            }
            catch (Exception ex)
            {
                Console.Error.Write($"Faltal error {ex.Message}");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
             .UseSerilog((ctx, logConfig) =>
             {
                 logConfig.ReadFrom.Configuration(ctx.Configuration);
             })
            .ConfigureServices((hostContext, services) =>
            {
                // configura la seccion que contiene los datos de conexion de masstransi
                services.Configure<RabbitMQConfig>(hostContext.Configuration.GetSection(RabbitMQConfig.SECTION_NAME));

                // crea el db context que persiste las maquinas de estado de saga
                // Comando para generasr el migration (ya esta generado)
                // $ dotnet ef migrations add InitialCreate -c JobServiceSagaDbContext
                // Comando apr aactualizar la base de datos
                // $ dotnet ef database update -c JobServiceSagaDbContext
                // comand opara generar el script idempotente
                // dotnet ef migrations script -c JobServiceSagaDbContext -i
                services.AddDbContext<JobServiceSagaDbContext>(builder =>
                  builder.UseSqlServer(hostContext.Configuration.GetConnectionString("JobServiceSql"), m =>
                  {
                      m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                      m.MigrationsHistoryTable($"__{nameof(JobServiceSagaDbContext)}");
                  }));


                services.AddMassTransit(x =>
                {

                    x.AddDelayedMessageScheduler();

                    x.AddConsumersFromNamespaceContaining<DoJobConsumer>();

                    x.AddRequestClient<IJobDone>();
                    // maquina de estados qye rastrea el estado de lso jobs
                    x.AddSagaRepository<JobSaga>()
                        .EntityFrameworkRepository(r =>
                        {
                            r.ExistingDbContext<JobServiceSagaDbContext>();

                            // este lockstatement solo aplica a postgres, en sql no encontre NADA
                            // aumo que el motor esta haciendo todo el trabaco de ACID
                            //r.LockStatementProvider = new PostgresLockStatementProvider();
                        });

                    // rastrea los tipos de los jobs
                    x.AddSagaRepository<JobTypeSaga>()
                        .EntityFrameworkRepository(r =>
                        {
                            r.ExistingDbContext<JobServiceSagaDbContext>();
                            //r.LockStatementProvider = new PostgresLockStatementProvider();
                        });

                    //rastrea los intentos de cada job
                    x.AddSagaRepository<JobAttemptSaga>()
                        .EntityFrameworkRepository(r =>
                        {
                            r.ExistingDbContext<JobServiceSagaDbContext>();
                            //r.LockStatementProvider = new PostgresLockStatementProvider();
                        });

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

                        var options = new ServiceInstanceOptions()
                            .EnableJobServiceEndpoints()
                            .SetEndpointNameFormatter(context.GetService<IEndpointNameFormatter>() ?? KebabCaseEndpointNameFormatter.Instance);

                        cfg.ServiceInstance(options, instance =>
                        {

                            // que son los endpoint? colas de mensajes

                            // endpoints para la parte de jobs
                            instance.ConfigureJobServiceEndpoints(js =>
                            {
                                //js.SagaPartitionCount = 1; (efcore tiene concurrencia pesimista por defecto, no es recomendable)


                                // mantiene limpia la base de datos de jobs completos
                                js.FinalizeCompleted = true;

                                js.ConfigureSagaRepositories(context);
                            });

                            // enpoints para todo el resto de cosas que no sean jobs
                            instance.ConfigureEndpoints(context);
                        });
                    });
                });
                services.AddMassTransitHostedService();

            });
    }
}
