using Confluent.Kafka;
using Shared.Interfaces;
using Shared.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Shared
{
    public class FakeConsumerWrapper : ConsumerWrapper<Null, string>
    {
        protected FakeConsumerWrapper(KafkaConsumerConfig config) : base(config)
        {
        }

        public static FakeConsumerWrapper New(KafkaConsumerConfig config)
        {
            FakeConsumerWrapper cw = new FakeConsumerWrapper(config);
            return cw;
        }

        public override Task CommitMessageHandler(IEnumerable<TopicPartitionOffset> offsets)
        {
            return Task.CompletedTask;
        }

        public override void Poll()
        {
            if (_buffer.Count < config.BufferLimit)
            {
                XmlSerializer xsSubmit = new XmlSerializer(typeof(ExtensionToSomeContract));
                XmlSerializer xsSubmit2 = new XmlSerializer(typeof(SomeContract));
                for (var i = 0; i < 200; i++)
                {
                    Random r = new Random((int)DateTime.Now.Ticks);
                    SomeContract sc = new SomeContract() { Id = r.Next(0, int.MaxValue), Timestamp = DateTime.Now };
                    ExtensionToSomeContract ex = new ExtensionToSomeContract() { Id = sc.Id, Timestamp = sc.Timestamp, Payload = Guid.NewGuid().ToString() };

                    using (var sww = new StringWriter())
                    {
                        using (XmlWriter writer = XmlWriter.Create(sww))
                        {
                            xsSubmit.Serialize(writer, ex);
                            sc.Content = sww.ToString(); // Your XML
                        }
                    }

                    using (var sww = new StringWriter())
                    {
                        using (XmlWriter writer = XmlWriter.Create(sww))
                        {
                            xsSubmit2.Serialize(writer, sc);
                            var ts = new Timestamp(DateTime.Now, TimestampType.CreateTime);
                            var s = sww.ToString();
                            this.Consumer_OnMessage(this, new Message<Null, string>("", 1, 1, null, s, ts, null));
                        }
                    }
                }
            }
            else
            {
                FlushBuffer();
            }
        }

        public override IConsumerWrapper Run(CancellationToken token)
        {
            return this;
        }
    }
}