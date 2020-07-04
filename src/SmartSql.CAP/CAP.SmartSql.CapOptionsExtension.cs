// Copyright (c) .NET Core Community. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using DotNetCore.CAP;
using DotNetCore.CAP.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SmartSql.CAP
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

            //Add SmartSqlOptions
            services.Configure(_configure);
        } 
    }
}