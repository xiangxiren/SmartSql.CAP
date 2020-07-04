using Sample.Kafka.MySql.Domain;
using SmartSql;
using SmartSql.DyRepository;

namespace Sample.Kafka.MySql.Repository
{
    public interface IPersonRepository : IInsertAsync<Person>, IGetEntityAsync<Person, long>
    {
        ISqlMapper SqlMapper { get; }
    }
}