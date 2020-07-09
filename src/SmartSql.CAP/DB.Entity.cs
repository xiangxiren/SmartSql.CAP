using System;

namespace SmartSql.CAP
{
    public class MessagesOfNeedRetry
    {
        public long Id { get; set; }

        public string Content { get; set; }

        public int Retries { get; set; }

        public DateTime Added { get; set; }
    }

    public class TimelineCounter
    {
        public string Key { get; set; }

        public int Count { get; set; }
    }
}