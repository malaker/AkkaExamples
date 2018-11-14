using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Shared.Interfaces;
using System.Collections.Generic;

namespace Shared
{
    public class ConsumerWrapperFactory : IConsumerWrapperFactory
    {
        private KafkaConsumerConfig config;

        public ConsumerWrapperFactory(KafkaConsumerConfig config)
        {
            this.config = config;
        }

        public IConsumerWrapper Create()
        {
            return ConsumerWrapper<Null, string>.New(config, new StringDeserializer(System.Text.Encoding.UTF8)).Subscribe(config.Topics).WithPoolingTimeout(1000);
        }
    }

    public class FakeConsumerFactory : IConsumerWrapperFactory
    {
        public IConsumerWrapper Create()
        {
            return FakeConsumerWrapper.New(new KafkaConsumerConfig()).Subscribe(new List<string>() { "dummy" }).WithPoolingTimeout(1000); ;
        }
    }
}