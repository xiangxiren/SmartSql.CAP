using System;
using System.Threading.Tasks;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Mvc;
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

        public PersonController(ICapPublisher capBus, PersonService service)
        {
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
            await _capBus.PublishAsync("sample.kafka.mysql", new Person()
            {
                Id = 123,
                Name = "Bar"
            });

            return Ok();
        }

        [NonAction]
        [CapSubscribe("sample.kafka.mysql")]
        public async Task Subscriber(long id)
        {
            var person = await _service.GetByIdAsync(id);
            Console.WriteLine($@"{DateTime.Now} Subscriber invoked, Info: {person}");
        }

        [NonAction]
        [CapSubscribe("sample.kafka.mysql", Group = "group.test2")]
        public async Task Subscriber2(long id, [FromCap] CapHeader header)
        {
            var person = await _service.GetByIdAsync(id);
            Console.WriteLine($@"{DateTime.Now} Subscriber invoked, Info: {person}");
        }
    }
}
