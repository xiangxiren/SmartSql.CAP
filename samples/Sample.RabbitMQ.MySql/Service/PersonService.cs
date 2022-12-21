using System.Threading.Tasks;
using DotNetCore.CAP;
using Sample.RabbitMQ.MySql.Domain;
using Sample.RabbitMQ.MySql.Repository;
using SmartSql.AOP;
using SmartSql.CAP;

namespace Sample.RabbitMQ.MySql.Service
{
    public class PersonService
    {
        private readonly IPersonRepository _repository;
        private readonly ICapPublisher _publisher;

        public PersonService(IPersonRepository repository, ICapPublisher publisher)
        {
            _repository = repository;
            _publisher = publisher;
        }

        public async Task<Person> GetByIdAsync(int id) =>
            await _repository.GetByIdAsync(id);

        public async Task<int> InsertAsync(Person person) => await _repository.InsertAsync(person);

        [CapTransaction]
        public virtual async Task AopAddAsync()
        {
            var person = new Person { Name = "test1" };
            person.Id = await _repository.InsertAsync(person);

            await _publisher.PublishAsync("sample.rabbitmq.mysql", person.Id);
        }

        public async Task MtAddAsync()
        {
            using var trans = _repository.SqlMapper.BeginCapTransaction(_publisher);
            var person = new Person { Name = "test1" };
            person.Id = await _repository.InsertAsync(person);
            await _publisher.PublishAsync("sample.rabbitmq.mysql", person.Id);

            await trans.CommitAsync();
        }
    }
}
