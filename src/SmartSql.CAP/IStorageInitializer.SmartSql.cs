// Copyright (c) .NET Core Community. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using DotNetCore.CAP.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SmartSql.CAP
{
    public class SmartSqlStorageInitializer : IStorageInitializer
    {
        private readonly IOptions<SmartSqlOptions> _options;
        private readonly ILogger _logger;
        private readonly ICapRepository _capRepository;

        public SmartSqlStorageInitializer(
            ILogger<SmartSqlStorageInitializer> logger,
            IOptions<SmartSqlOptions> options,
            ICapRepository capRepository)
        {
            _options = options;
            _capRepository = capRepository;
            _logger = logger;
        }

        public virtual string GetPublishedTableName()
        {
            return $"{_options.Value.Schema}.published";
        }

        public virtual string GetReceivedTableName()
        {
            return $"{_options.Value.Schema}.received";
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await _capRepository.InitializeTablesAsync(_options.Value.Schema, GetReceivedTableName(), GetPublishedTableName());
            _logger.LogDebug("Ensuring all create database tables script are applied.");
        }
    }
}