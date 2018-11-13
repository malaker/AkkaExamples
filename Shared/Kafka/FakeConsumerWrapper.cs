using Confluent.Kafka;
using Shared.Interfaces;
using Shared.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Shared
{
    public class FakeConsumerWrapper : ConsumerWrapper
    {
        public static FakeConsumerWrapper New(System.Collections.Generic.IEnumerable<KeyValuePair<string, object>> config)
        {
            FakeConsumerWrapper cw = new FakeConsumerWrapper();
            return cw;
        }

        public override Task CommitMessageHandler(IEnumerable<TopicPartitionOffset> offsets)
        {
            return Task.CompletedTask;
        }

        public override void Poll()
        {
            if (_buffer.Count < _bufferLimit)
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
                            var bytes = Encoding.UTF8.GetBytes(s);

                            this.Consumer_OnMessage(this, new Message("", 1, 1, null, bytes, ts, null));
                        }
                    }

                    var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    using (var ms = new MemoryStream())
                    {
                        binaryFormatter.Serialize(ms, sc);
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