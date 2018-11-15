using Shared.Messages;
using System.Collections.Generic;

namespace Shared
{
    public class InsertOrUpdateSomeContract : InsertOrUpdateCommand
    {
        public List<SomeContract> Data { get; set; }
    }
}