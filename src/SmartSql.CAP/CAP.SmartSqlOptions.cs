// Copyright (c) .NET Core Community. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace SmartSql.CAP
{
    public class SmartSqlOptions
    {
        public const string DefaultSchema = "cap";

        /// <summary>
        /// Gets or sets the schema to use when creating database objects.
        /// </summary>
        public string Schema { get; set; } = DefaultSchema;

        /// <summary>
        /// Data version
        /// </summary>
        internal string Version { get; set; } = "v1";

        /// <summary>
        /// Gets or sets the database's connection string that will be used to store database entities.
        /// </summary>
        public string ConnectionString { get; set; }
    }
}