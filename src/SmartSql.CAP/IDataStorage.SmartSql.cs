using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotNetCore.CAP;
using DotNetCore.CAP.Internal;
using DotNetCore.CAP.Messages;
using DotNetCore.CAP.Monitoring;
using DotNetCore.CAP.Persistence;
using DotNetCore.CAP.Serialization;
using Microsoft.Extensions.Options;

namespace SmartSql.CAP
{
    public class SmartSqlDataStorage : IDataStorage
    {
        private readonly IOptions<CapOptions> _capOptions;
        private readonly IStorageInitializer _initializer;
        private readonly ISerializer _serializer;
        private readonly ICapRepository _capRepository;
        private readonly string _pubName;
        private readonly string _recName;

        public SmartSqlDataStorage(
            IOptions<CapOptions> capOptions,
            IStorageInitializer initializer,
            ISerializer serializer,
            ICapRepository capRepository)
        {
            _capOptions = capOptions;
            _initializer = initializer;
            _serializer = serializer;
            _capRepository = capRepository;
            _pubName = initializer.GetPublishedTableName();
            _recName = initializer.GetReceivedTableName();
        }

        public async Task ChangePublishStateAsync(MediumMessage message, StatusName state) =>
            await _capRepository.ChangeMessageStateAsync(_pubName, message.DbId, message.Retries, message.ExpiresAt,
                state.ToString("G"), _serializer.Serialize(message.Origin));

        public async Task ChangeReceiveStateAsync(MediumMessage message, StatusName state) =>
            await _capRepository.ChangeMessageStateAsync(_recName, message.DbId, message.Retries, message.ExpiresAt,
                state.ToString("G"), _serializer.Serialize(message.Origin));

        public MediumMessage StoreMessage(string name, Message content, object dbTransaction = null)
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

            _capRepository.InsertPublishedMessage(_pubName, message.DbId, _capOptions.Value.Version, name,
                message.Content, message.Retries, message.Added, message.ExpiresAt, nameof(StatusName.Scheduled));

            return message;
        }

        public void StoreReceivedExceptionMessage(string name, string group, string content)
        {
            _capRepository.InsertReceivedMessage(_recName, SnowflakeId.Default().NextId().ToString(),
                _capOptions.Value.Version, name, group, content, _capOptions.Value.FailedRetryCount, DateTime.Now,
                DateTime.Now.AddDays(15), nameof(StatusName.Failed));
        }

        public MediumMessage StoreReceivedMessage(string name, string group, Message message)
        {
            var mdMessage = new MediumMessage
            {
                DbId = SnowflakeId.Default().NextId().ToString(),
                Origin = message,
                Added = DateTime.Now,
                ExpiresAt = null,
                Retries = 0
            };

            _capRepository.InsertReceivedMessage(_recName, mdMessage.DbId, _capOptions.Value.Version, name, group,
                _serializer.Serialize(mdMessage.Origin), mdMessage.Retries, mdMessage.Added, mdMessage.ExpiresAt,
                nameof(StatusName.Failed));

            return mdMessage;
        }

        public async Task<int> DeleteExpiresAsync(string table, DateTime timeout, int batchCount = 1000,
            CancellationToken token = default) => await _capRepository.DeleteExpiresAsync(table, timeout, batchCount);

        public async Task<IEnumerable<MediumMessage>> GetPublishedMessagesOfNeedRetry() =>
            await GetMessagesOfNeedRetryAsync(_pubName);

        public async Task<IEnumerable<MediumMessage>> GetReceivedMessagesOfNeedRetry() =>
            await GetMessagesOfNeedRetryAsync(_recName);

        public IMonitoringApi GetMonitoringApi() => new SmartSqlMonitoringApi(_initializer, _capRepository);

        private async Task<IEnumerable<MediumMessage>> GetMessagesOfNeedRetryAsync(string tableName)
        {
            var list = await _capRepository.GetMessagesOfNeedRetryAsync(tableName,
                _capOptions.Value.FailedRetryCount, _capOptions.Value.Version, DateTime.Now.AddMinutes(-4).ToString("O"));

            var messages = new List<MediumMessage>();
            foreach (var messagesOfNeedRetry in list)
            {
                messages.Add(new MediumMessage
                {
                    DbId = messagesOfNeedRetry.Id.ToString(),
                    Origin = _serializer.Deserialize(messagesOfNeedRetry.Content),
                    Retries = messagesOfNeedRetry.Retries,
                    Added = messagesOfNeedRetry.Added
                });
            }

            return messages;
        }
    }
}