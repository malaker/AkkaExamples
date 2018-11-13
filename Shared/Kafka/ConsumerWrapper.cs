using Akka.Actor;
using Confluent.Kafka;
using Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Shared
{
    public class ConsumerWrapper : IDisposable, IConsumerWrapper
    {
        protected Consumer consumer;
        protected List<IActorRef> observers;
        protected IEnumerable<string> topics;
        protected int timeout = 1000;
        protected int commitPeriod = 1;
        protected long lastCommitedOffset = 0;
        protected List<Message> _buffer = new List<Message>();
        protected int _bufferLimit = 2000;

        protected ConsumerWrapper()
        {
            this.observers = new List<IActorRef>();
        }

        protected ConsumerWrapper(Consumer consumer)
        {
            this.consumer = consumer;
            this.observers = new List<IActorRef>();
        }

        public static IConsumerWrapper New(IEnumerable<KeyValuePair<string, object>> config)
        {
            Consumer consumer = new Consumer(config);
            ConsumerWrapper cw = new ConsumerWrapper(consumer);
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
            if (_buffer.Count < _bufferLimit)
            {
                consumer.Poll(this.timeout);
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
            this.timeout = timeout;
            return this;
        }

        public virtual IConsumerWrapper WithCommitPeriod(int commitPeriod)
        {
            this.commitPeriod = commitPeriod;
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

        protected virtual void Consumer_OnMessage(object sender, Message e)
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