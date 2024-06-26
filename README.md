# SmartSql.CAP

Support CAP extension for SmartSql

## Getting Started

### Nuget

You can run the following command to install the SmartSql.CAP in your project.

```
PM> Install-Package SmartSql.CAP
```

### Configuration
config SmartSql and CAP in your Startup.cs

```
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers();

    services
        .AddSmartSql((provider, builder) => { builder.UseProperties(Configuration); })
        .AddRepositoryFromAssembly(options => { options.AssemblyString = "Sample.Kafka.MySql"; })
        .AddCapRepository();

    services.AddCap(options =>
    {
        options.UseSmartSql();
        options.UseKafka("localhost:9092");
        options.UseDashboard();
    });
}
```

Config SmartSqlMapConfig.xml

* MySql 8.0- (Not support `FOR UPDATE SKIP LOCKED`)

``` xml
<SmartSqlMaps>
  <SmartSqlMap Path="Maps" Type="Directory"/>
  <SmartSqlMap Path="SmartSql.CAP.Maps.CapMySql.xml,SmartSql.CAP" Type="Embedded" />
</SmartSqlMaps>
```

* MySql 8.0+ (Support `FOR UPDATE SKIP LOCKED`)

``` xml
<SmartSqlMaps>
  <SmartSqlMap Path="Maps" Type="Directory"/>
  <SmartSqlMap Path="SmartSql.CAP.Maps.CapMySql8.xml,SmartSql.CAP" Type="Embedded" />
</SmartSqlMaps>
```

* PostgreSQL

``` xml
<SmartSqlMaps>
  <SmartSqlMap Path="Maps" Type="Directory"/>
  <SmartSqlMap Path="SmartSql.CAP.Maps.CapPostgreSql.xml,SmartSql.CAP" Type="Embedded" />
</SmartSqlMaps>
```

* MSSQL 2008

``` xml
<SmartSqlMaps>
  <SmartSqlMap Path="Maps" Type="Directory"/>
  <SmartSqlMap Path="SmartSql.CAP.Maps.CapSqlServer2008.xml,SmartSql.CAP" Type="Embedded" />
</SmartSqlMaps>
```

* MSSQL 2012+

``` xml
<SmartSqlMaps>
  <SmartSqlMap Path="Maps" Type="Directory"/>
  <SmartSqlMap Path="SmartSql.CAP.Maps.CapSqlServer.xml,SmartSql.CAP" Type="Embedded" />
</SmartSqlMaps>
```

## Publish message With Transaction

```
public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly ICapPublisher _capBus;

    public UserService(IUserRepository userRepository, ICapPublisher capPublisher)
    {
        _userRepository = userRepository;
        _capBus = capPublisher;
    }
	
    [CapTransaction(AutoCommit = false)]
    public virtual async Task AopAddAsync()
    {
        var person = new Person { Id = DateTime.Now.ToFileTimeUtc(), Name = "test1" };
        await _repository.InsertAsync(person);

        await _publisher.PublishAsync("sample.kafka.mysql", person.Id);
    }

    public async Task MtAddAsync()
    {
        using var trans = _repository.SqlMapper.BeginCapTransaction(_publisher);
        
        var person = new Person { Id = DateTime.Now.ToFileTimeUtc(), Name = "test1" };
        await _repository.InsertAsync(person);
        await _publisher.PublishAsync("sample.kafka.mysql", person.Id);

        await trans.CommitAsync();
    }

    public async Task MtAdd1Async()
    {
        await _repository.SqlMapper.CapTransactionWrapAsync(_publisher, async() =>
        {
            var person = new Person { Name = "test1" };
            person.Id = await _repository.InsertAsync(person);
            await _publisher.PublishAsync("sample.rabbitmq.mysql", person.Id);
        });
    }
}

```
