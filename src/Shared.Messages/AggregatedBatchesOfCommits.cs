using System;
using System.Collections.Generic;

namespace Shared.Messages
{
    [Serializable]
    public class AggregatedBatchesOfCommits
    {
        public List<BatchOffsetCommits> Batches { get; set; }
    }
}