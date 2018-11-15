using System;

namespace Shared.Messages
{
    [Serializable]
    public class SomeContract
    {
        public long Id { get; set; }

        public DateTime Timestamp { get; set; }

        public string Content { get; set; }
    }
}