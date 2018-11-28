using Akka.Actor;
using Akka.Event;
using Confluent.Kafka;
using MediatR;
using Shared.Messages;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Shared
{
    public class SimpleAkkaMessageProcessor : ReceiveActor
    {
        public SimpleAkkaMessageProcessor(IMediator mediator)
        {
            this.mediator = mediator;
            this.serializer = new XmlSerializer(typeof(SomeContract));
            Receive<Confluent.Kafka.Message<Null, string>>(MessageHandler);
            Receive<FlushBufferMessage>(m => FlushHandler(m));
            Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(1000, 2000, Self, new FlushBufferMessage(), Self);
        }

        public List<SomeContract> Buffer = new List<SomeContract>();
        public List<TopicPartitionOffset> OffsetPartition = new List<TopicPartitionOffset>();
        private IMediator mediator;
        private XmlSerializer serializer;

        private bool MessageHandler(Confluent.Kafka.Message<Null, string> msg)
        {
            using (StringReader strReader = new StringReader(msg.Value))
            {
                var someContractObj = (SomeContract)serializer.Deserialize(strReader);
                this.Buffer.Add(someContractObj);
                this.OffsetPartition.Add(msg.TopicPartitionOffset);
            }
            return false;
        }

        private bool FlushHandler(FlushBufferMessage msg)
        {
            var log = Context.GetLogger();
            var coordinator = Context.System.ActorSelection("/user/akkaConsumerWrapper/messageRouter");
            if (Buffer.Any())
            {
                try
                {
                    this.mediator.Send(new InsertOrUpdateSomeContract() { Data = Buffer }).GetAwaiter().GetResult();
                }
                catch (SqlException ex)
                {
                    log.Error(ex.Message);
                }

                coordinator.Tell(new BatchOffsetCommits() { Commits = OffsetPartition.Select(m => new CommitMessage() { TopicPartitionOffset = m }).ToList() });

                Buffer.Clear();

                this.OffsetPartition.Clear();
            }

            return false;
        }
    }
}