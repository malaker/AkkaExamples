using Shared.Interfaces;
using System.Collections.Generic;

namespace Shared
{
    public class ConsumerWrapperFactory : IConsumerWrapperFactory
    {
        private KafkaConfig config;

        public ConsumerWrapperFactory(KafkaConfig config)
        {
            this.config = config;
        }

        public IConsumerWrapper Create()
        {
            return ConsumerWrapper.New(config.Settings).Subscribe(config.Topics).WithCommitPeriod(5).WithPoolingTimeout(1000);
        }
    }

    public class FakeConsumerFactory : IConsumerWrapperFactory
    {
        public IConsumerWrapper Create()
        {
            return FakeConsumerWrapper.New(new List<KeyValuePair<string, object>>()).Subscribe(new List<string>() { "dummy" }).WithCommitPeriod(5).WithPoolingTimeout(1000); ;
        }
    }
}