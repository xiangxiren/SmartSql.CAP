using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetCore.CAP.Monitoring;
using DotNetCore.CAP.Persistence;

namespace SmartSql.CAP
{
    public interface ICapRepository
    {
        Task InitializeTablesAsync(string schema, string receivedTableName, string publishedTableName);

        StatisticsDto GetStatistics(string receivedTableName, string publishedTableName);

        List<TimelineCounter> GetTimelineStats(string tableName, string statusName, string minKey,
            string maxKey);

        List<MessageDto> QueryMessages(string tableName, string name, string group, string content, string statusName,
            int limit, int offset);

        int GetNumberOfMessage(string tableName, string statusName);

        Task<MediumMessage> GetMessageAsync(string tableName, long id);

        Task ChangeMessageStateAsync(string tableName, string id, int retries, DateTime? expiresAt, string statusName);

        void InsertPublishedMessage(string tableName, string id, string version, string name, string content,
            int retries, DateTime added, DateTime? expiresAt, string statusName);

        void InsertReceivedMessage(string tableName, string id, string version, string name, string group,
            string content, int retries, DateTime added, DateTime? expiresAt, string statusName);

        Task<int> DeleteExpiresAsync(string tableName, DateTime timeout, int batchCount);

        Task<List<MessagesOfNeedRetry>> GetMessagesOfNeedRetryAsync(string tableName, int retries, string version,
            string added);
    }
}