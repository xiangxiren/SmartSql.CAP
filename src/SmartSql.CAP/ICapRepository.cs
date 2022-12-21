using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetCore.CAP.Internal;
using DotNetCore.CAP.Monitoring;
using DotNetCore.CAP.Persistence;

namespace SmartSql.CAP;

public interface ICapRepository
{
    ISqlMapper SqlMapper { get; }

    Task InitializeTablesAsync(string schema, string receivedTableName, string publishedTableName);

    Task<StatisticsDto> GetStatisticsAsync(string receivedTableName, string publishedTableName);

    Task<List<TimelineCounter>> GetTimelineStatsAsync(string tableName, string statusName, string? minKey,
        string? maxKey);

    Task<List<MessageDto>> QueryMessagesAsync(string tableName, string? name, string? group, string? content, string? statusName,
        int limit, int offset);

    Task<long> GetRecordAsync(string tableName, string? name, string? group, string? content, string? statusName);

    Task<int> GetNumberOfMessageAsync(string tableName, string statusName);

    Task<MediumMessage> GetMessageAsync(string tableName, long id);

    Task ChangePublishStateToDelayedAsync(string tableName, string[] ids, StatusName statusName = StatusName.Delayed);

    Task ChangeMessageStateAsync(string tableName, string id, int retries, DateTime? expiresAt, string statusName, string content);

    Task InsertPublishedMessageAsync(string tableName, string id, string version, string name, string content,
        int retries, DateTime added, DateTime? expiresAt, string statusName);

    Task InsertReceivedMessageAsync(string tableName, string id, string version, string name, string group,
        string content, int retries, DateTime added, DateTime? expiresAt, string statusName);

    Task<int> DeleteExpiresAsync(string tableName, DateTime timeout, int batchCount);

    Task<List<MessagesOfNeedRetry>> GetMessagesOfNeedRetryAsync(string tableName, int retries, string version,
        DateTime added);

    Task<List<MessagesOfNeedRetry>> GetMessagesOfDelayedAsync(string tableName, string version,
        DateTime twoMinutesLater, DateTime oneMinutesAgo);
}