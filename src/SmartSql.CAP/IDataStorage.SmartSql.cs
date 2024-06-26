﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNetCore.CAP;
using DotNetCore.CAP.Internal;
using DotNetCore.CAP.Messages;
using DotNetCore.CAP.Monitoring;
using DotNetCore.CAP.Persistence;
using DotNetCore.CAP.Serialization;
using Microsoft.Extensions.Options;

namespace SmartSql.CAP;

public class SmartSqlDataStorage : IDataStorage
{
    private readonly IOptions<CapOptions> _capOptions;
    private readonly IStorageInitializer _initializer;
    private readonly ISerializer _serializer;
    private readonly ISnowflakeId _snowflakeId;
    private readonly ICapRepository _capRepository;
    private readonly string _pubName;
    private readonly string _recName;
    private readonly string _lockName;

    public SmartSqlDataStorage(
        IOptions<CapOptions> capOptions,
        IStorageInitializer initializer,
        ISerializer serializer,
        ISnowflakeId snowflakeId,
        ICapRepository capRepository)
    {
        _capOptions = capOptions;
        _initializer = initializer;
        _serializer = serializer;
        _snowflakeId = snowflakeId;
        _capRepository = capRepository;
        _pubName = initializer.GetPublishedTableName();
        _recName = initializer.GetReceivedTableName();
        _lockName = initializer.GetLockTableName();
    }

    public async Task<bool> AcquireLockAsync(string key, TimeSpan ttl, string instance,
        CancellationToken token = new()) =>
        await _capRepository.AcquireLockAsync(_lockName, key, DateTime.Now.Subtract(ttl), instance, DateTime.Now) > 0;

    public async Task ReleaseLockAsync(string key, string instance, CancellationToken token = new()) =>
        await _capRepository.ReleaseLockAsync(_lockName, key, instance, DateTime.MinValue);

    public async Task RenewLockAsync(string key, TimeSpan ttl, string instance, CancellationToken token = new()) =>
        await _capRepository.RenewLockAsync(_lockName, key, ttl.TotalSeconds, instance);

    public async Task ChangePublishStateToDelayedAsync(string[] ids) =>
        await _capRepository.ChangePublishStateToDelayedAsync(_pubName, ids)
            .ConfigureAwait(false);

    public async Task ChangePublishStateAsync(MediumMessage message, StatusName state, object? transaction = null) =>
        await _capRepository.ChangeMessageStateAsync(_pubName, message.DbId, message.Retries, message.ExpiresAt,
            state.ToString("G"), _serializer.Serialize(message.Origin))
            .ConfigureAwait(false);

    public async Task ChangeReceiveStateAsync(MediumMessage message, StatusName state) =>
        await _capRepository.ChangeMessageStateAsync(_recName, message.DbId, message.Retries, message.ExpiresAt,
            state.ToString("G"), _serializer.Serialize(message.Origin))
            .ConfigureAwait(false);

    public async Task<MediumMessage> StoreMessageAsync(string name, Message content, object? dbTransaction = null)
    {
        var message = new MediumMessage
        {
            DbId = content.GetId(),
            Origin = content,
            Content = _serializer.Serialize(content),
            Added = DateTime.Now,
            ExpiresAt = null,
            Retries = 0
        };

        await _capRepository.InsertPublishedMessageAsync(_pubName, message.DbId, _capOptions.Value.Version, name,
                message.Content, message.Retries, message.Added, message.ExpiresAt, nameof(StatusName.Scheduled))
            .ConfigureAwait(false);

        return message;
    }

    public async Task StoreReceivedExceptionMessageAsync(string name, string group, string content)
    {
        await _capRepository.InsertReceivedMessageAsync(_recName, _snowflakeId.NextId().ToString(),
                _capOptions.Value.Version, name, group, content, _capOptions.Value.FailedRetryCount, DateTime.Now,
                DateTime.Now.AddSeconds(_capOptions.Value.FailedMessageExpiredAfter), nameof(StatusName.Failed))
            .ConfigureAwait(false);
    }

    public async Task<MediumMessage> StoreReceivedMessageAsync(string name, string group, Message message)
    {
        var mdMessage = new MediumMessage
        {
            DbId = _snowflakeId.NextId().ToString(),
            Origin = message,
            Added = DateTime.Now,
            ExpiresAt = null,
            Retries = 0
        };

        await _capRepository.InsertReceivedMessageAsync(_recName, mdMessage.DbId, _capOptions.Value.Version, name,
                group, _serializer.Serialize(mdMessage.Origin), mdMessage.Retries, mdMessage.Added,
                mdMessage.ExpiresAt, nameof(StatusName.Failed))
            .ConfigureAwait(false);

        return mdMessage;
    }

    public async Task<int> DeleteExpiresAsync(string table, DateTime timeout, int batchCount = 1000,
        CancellationToken token = default) =>
        await _capRepository.DeleteExpiresAsync(table, timeout, batchCount).ConfigureAwait(false);

    public async Task<IEnumerable<MediumMessage>> GetPublishedMessagesOfNeedRetry(TimeSpan lookbackSeconds) =>
        await GetMessagesOfNeedRetryAsync(_pubName, lookbackSeconds).ConfigureAwait(false);

    public async Task<IEnumerable<MediumMessage>> GetReceivedMessagesOfNeedRetry(TimeSpan lookbackSeconds) =>
        await GetMessagesOfNeedRetryAsync(_recName, lookbackSeconds).ConfigureAwait(false);

    public async Task ScheduleMessagesOfDelayedAsync(Func<object, IEnumerable<MediumMessage>, Task> scheduleTask,
        CancellationToken token = default)
    {
        await _capRepository.SqlMapper.TransactionWrapAsync(IsolationLevel.ReadCommitted, async () =>
        {
            var list = await _capRepository.GetMessagesOfDelayedAsync(_pubName, _capOptions.Value.Version,
                    DateTime.Now.AddMinutes(2), DateTime.Now.AddMinutes(-1))
                .ConfigureAwait(false);

            var messageList = list.Select(messagesOfNeedRetry => new MediumMessage
            {
                DbId = messagesOfNeedRetry.Id.ToString(),
                Origin = _serializer.Deserialize(messagesOfNeedRetry.Content)!,
                Retries = messagesOfNeedRetry.Retries,
                Added = messagesOfNeedRetry.Added
            }).ToList();

            await scheduleTask(default!, messageList);
        });
    }

    public IMonitoringApi GetMonitoringApi() => new SmartSqlMonitoringApi(_initializer, _capRepository);

    private async Task<IEnumerable<MediumMessage>> GetMessagesOfNeedRetryAsync(string tableName, TimeSpan lookbackSeconds)
    {
        var fourMinAgo = DateTime.Now.Subtract(lookbackSeconds);
        var list = await _capRepository.GetMessagesOfNeedRetryAsync(tableName,
                _capOptions.Value.FailedRetryCount, _capOptions.Value.Version, fourMinAgo)
            .ConfigureAwait(false);

        return list.Select(messagesOfNeedRetry => new MediumMessage
        {
            DbId = messagesOfNeedRetry.Id.ToString(),
            Origin = _serializer.Deserialize(messagesOfNeedRetry.Content)!,
            Retries = messagesOfNeedRetry.Retries,
            Added = messagesOfNeedRetry.Added
        }).ToList();
    }
}