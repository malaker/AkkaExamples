using System;
using System.Collections.Generic;

namespace Shared.Messages
{
    [Serializable]
    public class BatchOffsetCommits
    {
        public List<CommitMessage> Commits { get; set; }
    }
}