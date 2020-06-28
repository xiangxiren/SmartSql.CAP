# SmartSql.CAP

Support CAP extension for Smart.Sql

## Getting Started

You can run the following command to install the CAP in your project.

```
PM> Install-Package SmartSql.CAP
```

## AOP Transaction

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
	public virtual long AddWithTran(User user)
	{
        _capBus.Publish("xxx.services.show.time", DateTime.Now);
		return _userRepository.Insert(user);
	}
}

```