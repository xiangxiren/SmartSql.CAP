using System;
using System.Threading.Tasks;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sample.RabbitMQ.MySql.Domain;
using Sample.RabbitMQ.MySql.Service;

namespace Sample.RabbitMQ.MySql.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PersonController : ControllerBase
    {
        private readonly ICapPublisher _capBus;
        private readonly PersonService _service;
        private readonly ILogger _logger;

        public PersonController(ILogger<PersonController> logger, ICapPublisher capBus, PersonService service)
        {
            _logger = logger;
            _capBus = capBus;
            _service = service;
        }

        [HttpGet("~/aop")]
        public async Task AopAdd() =>
            await _service.AopAddAsync();

        [HttpGet("~/mt")]
        public async Task MtAdd() =>
            await _service.MtAddAsync();

        [Route("~/without/transaction")]
        public async Task<IActionResult> WithoutTransaction()
        {
            var id = await _service.InsertAsync(new Person { Name = "test1" });
            await _capBus.PublishAsync("sample.rabbitmq.mysql", id);

            return Ok();
        }

        [Route("~/delay/{delaySeconds:int}")]
        public async Task<IActionResult> Delay(int delaySeconds)
        {
            var id = await _service.InsertAsync(new Person { Name = "test1" });
            await _capBus.PublishDelayAsync(TimeSpan.FromSeconds(delaySeconds), "sample.rabbitmq.mysql", id);

            return Ok();
        }

        [NonAction]
        [CapSubscribe("sample.rabbitmq.mysql")]
        public async Task Subscriber(int id)
        {
            var person = await _service.GetByIdAsync(id);
            _logger.LogInformation($@"{DateTime.Now} Subscriber invoked, Info: {person}");
        }

        [NonAction]
        [CapSubscribe("sample.rabbitmq.mysql", Group = "group.test2")]
        public async Task Subscriber2(int id, [FromCap] CapHeader header)
        {
            var person = await _service.GetByIdAsync(id);
            _logger.LogInformation($@"{DateTime.Now} Subscriber invoked, Info: {person}");
        }
    }
}
