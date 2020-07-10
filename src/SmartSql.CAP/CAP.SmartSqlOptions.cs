// ReSharper disable once CheckNamespace
namespace DotNetCore.CAP
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
        /// Get or set whether to execute SQL to create a table. Default is true
        /// </summary>
        public bool InitializeTable { get; set; } = true;
    }
}