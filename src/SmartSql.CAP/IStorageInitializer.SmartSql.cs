using System;
using System.Threading;
using System.Threading.Tasks;
using DotNetCore.CAP;
using DotNetCore.CAP.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SmartSql.CAP;

public class SmartSqlStorageInitializer : IStorageInitializer
{
    private readonly IOptions<CapOptions> _capOptions;
    private readonly IOptions<SmartSqlOptions> _options;
    private readonly ILogger _logger;
    private readonly ICapRepository _capRepository;

    public SmartSqlStorageInitializer(
        ILogger<SmartSqlStorageInitializer> logger,
        IOptions<SmartSqlOptions> options,
        IOptions<CapOptions> capOptions,
        ICapRepository capRepository)
    {
        _options = options;
        _capOptions = capOptions;
        _capOptions = capOptions;
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

    public string GetLockTableName()
    {
        return $"{_options.Value.Schema}.lock";
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        if (!_options.Value.InitializeTable)
        {
            _logger.LogDebug("The initial value is false, it will not execute SQL to create the table structure.");
            return;
        }

        await _capRepository
            .InitializeTablesAsync(_options.Value.Schema, GetReceivedTableName(), GetPublishedTableName(),
                _capOptions.Value.UseStorageLock, GetLockTableName(), $"publish_retry_{_capOptions.Value.Version}",
                $"received_retry_{_capOptions.Value.Version}", DateTime.MinValue)
            .ConfigureAwait(false);

        _logger.LogDebug("Ensuring all create database tables script are applied.");
    }
}