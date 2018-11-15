using Confluent.Kafka;
using System;

namespace Shared.Messages
{
    [Serializable]
    public class CommitMessage
    {
        public TopicPartitionOffset TopicPartitionOffset { get; set; }
    }
}