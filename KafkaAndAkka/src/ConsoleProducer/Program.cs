using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Shared.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ConsoleProducer
{
    internal class Program
    {
        public static bool PingHost(string nameOrAddress, bool throwExceptionOnError = false)
        {
            bool pingable = false;
            using (Ping pinger = new Ping())
            {
                try
                {
                    PingReply reply = pinger.Send(nameOrAddress);
                    pingable = reply.Status == IPStatus.Success;
                }
                catch (PingException e)
                {
                    if (throwExceptionOnError) throw e;
                    pingable = false;
                }
            }
            return pingable;
        }

        private static void Main(string[] args)
        {
         
            var ping=PingHost("kafka", true);
            string topic = Environment.GetEnvironmentVariable("KAFKA_TOPIC_NAME");
   
            int messagesToProduce = 1000000;

            var config = new Dictionary<string, object>() { { "bootstrap.servers", Environment.GetEnvironmentVariable("KAFKA_BROKER_LIST") } };

            using (var producer = new Producer<Null, string>(config.ToList(), null, new StringSerializer(System.Text.Encoding.UTF8)))
            {
                producer.OnError += Producer_OnError;
                producer.OnLog += Producer_OnLog;
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExtensionToSomeContract));
                XmlSerializer xsSubmit2 = new XmlSerializer(typeof(SomeContract));
                for (var i = 0; i < messagesToProduce; i++)
                {
                    Random r = new Random((int)DateTime.Now.Ticks);
                    SomeContract sc = new SomeContract() { Id = r.Next(0, int.MaxValue), Timestamp = DateTime.Now };
                    ExtensionToSomeContract ex = new ExtensionToSomeContract() { Id = sc.Id, Timestamp = sc.Timestamp, Payload = Guid.NewGuid().ToString() };

                    using (var sw = new StringWriter())
                    {
                        using (XmlWriter writer = XmlWriter.Create(sw))
                        {
                            xmlSerializer.Serialize(writer, ex);
                            sc.Content = sw.ToString();
                        }
                    }

                    using (var sw = new StringWriter())
                    {
                        using (XmlWriter writer = XmlWriter.Create(sw))
                        {
                            xsSubmit2.Serialize(writer, sc);

                            var messageToSend = sw.ToString();
         
                            var rr =  producer.ProduceAsync(topic, null, messageToSend).GetAwaiter().GetResult();
                            if (i % 100 == 0)
                            {
                                producer.Flush(100);
                            }
                           
                        }
                    }
                }
            }
        }

        private static void Producer_OnLog(object sender, LogMessage e)
        {
            Console.WriteLine(e.Message);
        }

        private static void Producer_OnError(object sender, Error e)
        {
            Console.WriteLine(e.Reason);
        }
    }
}