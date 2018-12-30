using Akka.Actor;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Shared
{
    public class ConsumerWrapper<K, V> : IDisposable, IConsumerWrapper
    {
        protected KafkaConsumerConfig config;
        protected Consumer<K, V> consumer;
        protected List<IActorRef> observers;
        protected IEnumerable<string> topics;

        protected int commitPeriod = 1;
        protected long lastCommitedOffset = 0;
        protected List<Message<K, V>> _buffer = new List<Message<K, V>>();

        protected ConsumerWrapper(KafkaConsumerConfig config)
        {
            this.config = config;
            this.observers = new List<IActorRef>();
        }

        protected ConsumerWrapper(Consumer<K, V> consumer, KafkaConsumerConfig config)
        {
            this.consumer = consumer;
            this.config = config;
            this.observers = new List<IActorRef>();
        }

        public static IConsumerWrapper New(KafkaConsumerConfig config, IDeserializer<V> valueDeserializer)
        {
            Consumer<K, V> consumer = new Consumer<K, V>(config.Settings, null, valueDeserializer);
            ConsumerWrapper<K, V> cw = new ConsumerWrapper<K, V>(consumer, config);
            return cw;
        }

        public virtual IConsumerWrapper Subscribe(IEnumerable<string> topics)
        {
            this.topics = topics;

            return this;
        }

        public virtual IConsumerWrapper WithMessageObserver(IActorRef observer)
        {
            this.observers.Add(observer);
            return this;
        }

        public virtual void Poll()
        {
            if (_buffer.Count < config.BufferLimit)
            {
                consumer.Poll(config.Timeout);
            }
            else
            {
              FlushBuffer();
            }
        }

        protected virtual void FlushBuffer()
        {
            this._buffer.ForEach(m =>
            {
                this.observers.ForEach(o =>
                {
                    o.Tell(m);
                });
            });
            this._buffer.Clear();
        }

        public virtual IConsumerWrapper WithPoolingTimeout(int timeout)
        {
            this.config.Timeout = timeout;
            return this;
        }

        public virtual IConsumerWrapper Run(CancellationToken token)
        {
            consumer.OnMessage += Consumer_OnMessage;
            consumer.OnError += Consumer_OnError;
            this.consumer.Subscribe(this.topics);
            return this;
        }

        protected virtual void Consumer_OnError(object sender, Error e)
        {
            this.observers.ForEach(o =>
            {
                o.Tell(e);
            });
        }

        protected virtual void Consumer_OnMessage(object sender, Message<K, V> e)
        {
            this._buffer.Add(e);
        }

        public void Dispose()
        {
            consumer.Dispose();
        }

        public virtual async Task CommitMessageHandler(IEnumerable<TopicPartitionOffset> offsets)
        {
            var committedOffsets = await consumer.CommitAsync(offsets);
        }
    }
}