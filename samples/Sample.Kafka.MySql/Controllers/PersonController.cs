using System;
using System.Threading.Tasks;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Mvc;
using Sample.Kafka.MySql.Domain;
using Sample.Kafka.MySql.Service;

namespace Sample.Kafka.MySql.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PersonController : ControllerBase
    {
        private readonly PersonService _service;

        public PersonController(PersonService service)
        {
            _service = service;
        }

        [HttpGet("~/aop")]
        public async Task AopAdd() =>
            await _service.AopAddAsync();

        [HttpGet("~/mt")]
        public async Task MtAdd() =>
            await _service.MtAddAsync();

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
