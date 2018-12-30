using System;
using System.Collections.Generic;

namespace Shared
{
    public class KafkaConsumerConfig
    {
        public int Timeout { get; set; } = 1000;
        public int BufferLimit { get; set; } = 5000;
        public List<string> Topics { get; set; } = new List<string>() { Environment.GetEnvironmentVariable("KAFKA_TOPIC_NAME") };

        public Dictionary<string, object> Settings = new Dictionary<string, object>()
            {
                {"bootstrap.servers", Environment.GetEnvironmentVariable("KAFKA_BROKER_LIST") },
                {"client.id","test1" },
                {"group.id","test1g" }
            };
    }
}