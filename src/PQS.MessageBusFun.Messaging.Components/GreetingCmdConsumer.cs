using MassTransit;
using Microsoft.Extensions.Logging;
using PQS.MessageBusFun.Messaging.Contracts;
using System.Threading.Tasks;

namespace PQS.MessageBusFun.Messaging.Components
{
    public class GreetingCmdConsumer : IConsumer<IGreetingCmd>
    {
        readonly ILogger<GreetingCmdConsumer> _logger;
        public GreetingCmdConsumer(ILogger<GreetingCmdConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<IGreetingCmd> context)
        {
            _logger.LogInformation("Received Text: {Text}", context.Message.Name);

            return context.RespondAsync(new GreetingCmdResponse
            {
                Saludo = $"Hola {context.Message.Name}"
            }); ;
        }
    }
}
