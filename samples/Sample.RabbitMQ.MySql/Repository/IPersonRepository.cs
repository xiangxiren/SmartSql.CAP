using Sample.RabbitMQ.MySql.Domain;
using SmartSql;
using SmartSql.DyRepository;

namespace Sample.RabbitMQ.MySql.Repository
{
    public interface IPersonRepository : IInsertAsync<Person>, IGetEntityAsync<Person, int>
    {
        ISqlMapper SqlMapper { get; }
    }
}