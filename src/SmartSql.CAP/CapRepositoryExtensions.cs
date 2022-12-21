using SmartSql;
using SmartSql.DIExtension;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class CapRepositoryExtensions
{
    public static SmartSqlDIBuilder AddCapRepository(this SmartSqlDIBuilder builder,
        string alias = SmartSqlBuilder.DEFAULT_ALIAS)
    {
        builder.AddRepositoryFromAssembly(options =>
        {
            options.AssemblyString = typeof(CapRepositoryExtensions).Assembly.GetName().Name;
            options.SmartSqlAlias = alias;
        });

        return builder;
    }
}