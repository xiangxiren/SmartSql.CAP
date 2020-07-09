using System;
using DotNetCore.CAP.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmartSql.CAP;

// ReSharper disable once CheckNamespace
namespace DotNetCore.CAP
{
    internal class MySqlCapOptionsExtension : ICapOptionsExtension
    {
        private readonly Action<SmartSqlOptions> _configure;

        public MySqlCapOptionsExtension(Action<SmartSqlOptions> configure)
        {
            _configure = configure;
        }

        public void AddServices(IServiceCollection services)
        {
            services.AddSingleton<CapStorageMarkerService>();
            services.AddSingleton<IDataStorage, SmartSqlDataStorage>();
            
            services.TryAddSingleton<IStorageInitializer, SmartSqlStorageInitializer>();
            services.AddTransient<ICapTransaction, SmartSqlCapTransaction>();

            services.Configure(_configure);
        } 
    }
}