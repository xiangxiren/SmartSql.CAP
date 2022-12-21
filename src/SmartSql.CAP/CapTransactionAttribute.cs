using System;
using System.Data;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using DotNetCore.CAP;
using Microsoft.Extensions.DependencyInjection;
using SmartSql.CAP;
using SmartSql.Exceptions;

// ReSharper disable once CheckNamespace
namespace SmartSql.AOP;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class CapTransactionAttribute : AbstractInterceptorAttribute
{
    public string Alias { get; set; } = SmartSqlBuilder.DEFAULT_ALIAS;

    public IsolationLevel Level { get; set; } = IsolationLevel.Unspecified;

    public bool AutoCommit { get; set; } = false;

    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        var sessionStore = context.ServiceProvider.GetSessionStore(Alias);
        if (sessionStore == null)
        {
            throw new SmartSqlException($"can not find SmartSql instance by Alias:{Alias}.");
        }

        var publisher = context.ServiceProvider.GetService<ICapPublisher>();
        if (publisher == null)
        {
            throw new SmartSqlException($"Unable to resolve service for type {typeof(ICapPublisher).FullName}.");
        }

        var transcation = sessionStore.LocalSession?.Transaction;
        if (transcation != null)
        {
            throw new SmartSqlException(
                "SmartSqlMapper could not invoke BeginCapTransaction(). A CapTransaction is already existed.");
        }

        using (sessionStore)
        {
            await sessionStore.Open().CapTransactionWrapAsync(Level, publisher, async () =>
            {
                await next.Invoke(context);
            }, AutoCommit);
        }
    }
}