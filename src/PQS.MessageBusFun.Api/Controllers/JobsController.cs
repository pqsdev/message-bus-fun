using MassTransit;
using MassTransit.Contracts.JobService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PQS.MessageBusFun.Messaging.Contracts;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PQS.MessageBusFun.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {

        readonly IRequestClient<IDoJob> _client;
        readonly IRequestClient<IGreetingCmd> _greetingClient;
        readonly ILogger _logger;

        public JobsController(ILogger<JobsController> logger, IRequestClient<IDoJob> client, IRequestClient<IGreetingCmd> greetingClient)
        {

            _logger = logger;
            _client = client;
            _greetingClient = greetingClient;

        }


        [HttpPost("{path}")]
        public async Task<IActionResult> CresateJob(string path)
        {


            var groupId = NewId.Next().ToString();

            _logger.LogInformation($"Sending job: {path}:{groupId}");


            Response<JobSubmissionAccepted> response = await _client.GetResponse<JobSubmissionAccepted>(new
            {
                path,
                groupId,
                Index = 0,
                Count = 1
            });

            return Ok(new
            {
                response.Message.JobId,
                Path = path
            });
        }


        [HttpGet("{name}")]
        public async Task<ActionResult<GreetingCmdResponse>> GetSaludo(string name)
        {


            Response<GreetingCmdResponse> response = await _greetingClient.GetResponse<GreetingCmdResponse>(new { name });

            return Ok(response.Message);
        }
    }
}
