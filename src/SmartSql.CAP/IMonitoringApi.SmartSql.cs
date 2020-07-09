using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCore.CAP.Internal;
using DotNetCore.CAP.Messages;
using DotNetCore.CAP.Monitoring;
using DotNetCore.CAP.Persistence;

namespace SmartSql.CAP
{
    internal class SmartSqlMonitoringApi : IMonitoringApi
    {
        private readonly ICapRepository _capRepository;
        private readonly string _pubName;
        private readonly string _recName;

        public SmartSqlMonitoringApi(IStorageInitializer initializer, ICapRepository capRepository)
        {
            _capRepository = capRepository;
            _pubName = initializer.GetPublishedTableName();
            _recName = initializer.GetReceivedTableName();
        }

        public StatisticsDto GetStatistics() =>
            _capRepository.GetStatistics(_recName, _pubName);

        public IDictionary<DateTime, int> HourlyFailedJobs(MessageType type)
        {
            var tableName = type == MessageType.Publish ? _pubName : _recName;
            return GetHourlyTimelineStats(tableName, nameof(StatusName.Failed));
        }

        public IDictionary<DateTime, int> HourlySucceededJobs(MessageType type)
        {
            var tableName = type == MessageType.Publish ? _pubName : _recName;
            return GetHourlyTimelineStats(tableName, nameof(StatusName.Succeeded));
        }

        public IList<MessageDto> Messages(MessageQueryDto queryDto)
        {
            var tableName = queryDto.MessageType == MessageType.Publish ? _pubName : _recName;

            return _capRepository.QueryMessages(tableName, queryDto.Name, queryDto.Group, queryDto.Content,
                queryDto.StatusName, queryDto.PageSize, queryDto.CurrentPage * queryDto.PageSize);
        }

        public int PublishedFailedCount() =>
            _capRepository.GetNumberOfMessage(_pubName, nameof(StatusName.Failed));

        public int PublishedSucceededCount() =>
            _capRepository.GetNumberOfMessage(_pubName, nameof(StatusName.Succeeded));

        public int ReceivedFailedCount() =>
            _capRepository.GetNumberOfMessage(_recName, nameof(StatusName.Failed));

        public int ReceivedSucceededCount() =>
            _capRepository.GetNumberOfMessage(_recName, nameof(StatusName.Succeeded));

        private Dictionary<DateTime, int> GetHourlyTimelineStats(string tableName, string statusName)
        {
            var endDate = DateTime.Now;
            var dates = new List<DateTime>();
            for (var i = 0; i < 24; i++)
            {
                dates.Add(endDate);
                endDate = endDate.AddHours(-1);
            }

            var keyMaps = dates.ToDictionary(x => x.ToString("yyyy-MM-dd-HH"), x => x);

            return GetTimelineStats(tableName, statusName, keyMaps);
        }

        private Dictionary<DateTime, int> GetTimelineStats(
            string tableName,
            string statusName,
            IDictionary<string, DateTime> keyMaps)
        {
            var valuesMap =
                _capRepository.GetTimelineStats(tableName, statusName, keyMaps.Keys.Min(), keyMaps.Keys.Max())
                    .ToDictionary(x => x.Key, x => x.Count);

            foreach (var key in keyMaps.Keys)
            {
                if (!valuesMap.ContainsKey(key))
                {
                    valuesMap.Add(key, 0);
                }
            }

            var result = new Dictionary<DateTime, int>();
            for (var i = 0; i < keyMaps.Count; i++)
            {
                var value = valuesMap[keyMaps.ElementAt(i).Key];
                result.Add(keyMaps.ElementAt(i).Value, value);
            }

            return result;
        }

        public async Task<MediumMessage> GetPublishedMessageAsync(long id) =>
            await _capRepository.GetMessageAsync(_pubName, id);

        public async Task<MediumMessage> GetReceivedMessageAsync(long id) =>
            await _capRepository.GetMessageAsync(_recName, id);
    }
}