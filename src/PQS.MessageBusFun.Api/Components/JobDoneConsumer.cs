using MassTransit;
using Microsoft.Extensions.Logging;
using PQS.MessageBusFun.Messaging.Contracts;
using System.Threading.Tasks;

namespace PQS.MessageBusFun.Api.Components
{
    public class JobDoneConsumer : IConsumer<IJobDone>
    {
        readonly ILogger _logger;

        public JobDoneConsumer(ILogger<IJobDone> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<IJobDone> context)
        {

            _logger.LogInformation($"Completed job: {context.Message.GroupId}");

            return Task.CompletedTask;
        }
    }
}
