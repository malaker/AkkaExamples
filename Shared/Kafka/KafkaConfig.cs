using System.Collections.Generic;

namespace Shared
{
    public class KafkaConfig
    {
        public int Timeout { get; set; } = 1000;
        public int BufferLimit { get; set; } = 2000;
        public List<string> Topics { get; set; } = new List<string>() { "mytopic" };

        public Dictionary<string, object> Settings = new Dictionary<string, object>()
            {
                {"bootstrap.servers","localhost:9092" },
                {"client.id","test1" },
                {"group.id","test1g" }
            };
    }
}