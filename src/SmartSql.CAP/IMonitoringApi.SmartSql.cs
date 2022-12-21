using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCore.CAP.Internal;
using DotNetCore.CAP.Messages;
using DotNetCore.CAP.Monitoring;
using DotNetCore.CAP.Persistence;

namespace SmartSql.CAP;

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

    public async Task<MediumMessage?> GetPublishedMessageAsync(long id) =>
        await _capRepository.GetMessageAsync(_pubName, id).ConfigureAwait(false);

    public async Task<MediumMessage?> GetReceivedMessageAsync(long id) =>
        await _capRepository.GetMessageAsync(_recName, id).ConfigureAwait(false);

    public async Task<StatisticsDto> GetStatisticsAsync() =>
        await _capRepository.GetStatisticsAsync(_recName, _pubName).ConfigureAwait(false);

    public async Task<PagedQueryResult<MessageDto>> GetMessagesAsync(MessageQueryDto queryDto)
    {
        var tableName = queryDto.MessageType == MessageType.Publish ? _pubName : _recName;

        var list = await _capRepository.QueryMessagesAsync(tableName, queryDto.Name, queryDto.Group,
                queryDto.Content, queryDto.StatusName, queryDto.PageSize, queryDto.CurrentPage * queryDto.PageSize)
            .ConfigureAwait(false);

        var count = await _capRepository.GetRecordAsync(tableName, queryDto.Name, queryDto.Group, queryDto.Content,
                queryDto.StatusName)
            .ConfigureAwait(false);

        return new PagedQueryResult<MessageDto>
        {
            Items = list,
            PageIndex = queryDto.CurrentPage,
            PageSize = queryDto.PageSize,
            Totals = count
        };
    }

    public async Task<IDictionary<DateTime, int>> HourlyFailedJobs(MessageType type)
    {
        var tableName = type == MessageType.Publish ? _pubName : _recName;
        return await GetHourlyTimelineStatsAsync(tableName, nameof(StatusName.Failed));
    }

    public async Task<IDictionary<DateTime, int>> HourlySucceededJobs(MessageType type)
    {
        var tableName = type == MessageType.Publish ? _pubName : _recName;
        return await GetHourlyTimelineStatsAsync(tableName, nameof(StatusName.Succeeded));
    }

    public async ValueTask<int> PublishedFailedCount() =>
        await _capRepository.GetNumberOfMessageAsync(_pubName, nameof(StatusName.Failed)).ConfigureAwait(false);

    public async ValueTask<int> PublishedSucceededCount() =>
      await _capRepository.GetNumberOfMessageAsync(_pubName, nameof(StatusName.Succeeded)).ConfigureAwait(false);

    public async ValueTask<int> ReceivedFailedCount() =>
        await _capRepository.GetNumberOfMessageAsync(_recName, nameof(StatusName.Failed)).ConfigureAwait(false);

    public async ValueTask<int> ReceivedSucceededCount() =>
        await _capRepository.GetNumberOfMessageAsync(_recName, nameof(StatusName.Succeeded)).ConfigureAwait(false);

    private async Task<Dictionary<DateTime, int>> GetHourlyTimelineStatsAsync(string tableName, string statusName)
    {
        var endDate = DateTime.Now;
        var dates = new List<DateTime>();
        for (var i = 0; i < 24; i++)
        {
            dates.Add(endDate);
            endDate = endDate.AddHours(-1);
        }

        var keyMaps = dates.ToDictionary(x => x.ToString("yyyy-MM-dd-HH"), x => x);

        return await GetTimelineStatsAsync(tableName, statusName, keyMaps);
    }

    private async Task<Dictionary<DateTime, int>> GetTimelineStatsAsync(
        string tableName,
        string statusName,
        IDictionary<string, DateTime> keyMaps)
    {
        var valuesMap =
            (await _capRepository.GetTimelineStatsAsync(tableName, statusName, keyMaps.Keys.Min(),
                keyMaps.Keys.Max()).ConfigureAwait(false))
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
}