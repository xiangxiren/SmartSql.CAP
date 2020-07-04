// Copyright (c) .NET Core Community. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using DotNetCore.CAP;
using SmartSql.CAP;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class CapOptionsExtensions
    {
        public static CapOptions UseSmartSql(this CapOptions options)
        {
            return options.UseSmartSql(opts => { });
        }

        public static CapOptions UseSmartSql(this CapOptions options, Action<SmartSqlOptions> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            configure += x => x.Version = options.Version;

            options.RegisterExtension(new MySqlCapOptionsExtension(configure));

            return options;
        }
    }
}