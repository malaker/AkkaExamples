using System;

namespace Shared.Messages
{
    [Serializable()]
    public class ExtensionToSomeContract
    {
        public long Id { get; set; }

        public DateTime Timestamp { get; set; }

        public string Payload { get; set; }
    }
}