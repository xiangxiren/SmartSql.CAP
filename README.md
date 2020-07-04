# SmartSql.CAP

Support CAP extension for SmartSql

## Getting Started

### Nuget

You can run the following command to install the SmartSql.CAP in your project.

```
PM> Install-Package SmartSql.CAP
PM> Install-Package SmartSql.Cap.MySql
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

``` xml
<SmartSqlMaps>
    <SmartSqlMap Path="Maps" Type="Directory"/>
    <SmartSqlMap Path="SmartSql.Cap.MySql.Cap.xml,SmartSql.Cap.MySql" Type="Embedded" />
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
}

```
