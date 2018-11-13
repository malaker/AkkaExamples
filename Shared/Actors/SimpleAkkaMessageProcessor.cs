using Akka.Actor;
using Confluent.Kafka;
using MediatR;
using Shared.Messages;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            Receive<Confluent.Kafka.Message>(MessageHandler);
            Receive<FlushBufferMessage>(async m => await FlushHandler(m));
            Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(1000, 2000, Self, new FlushBufferMessage(), Self);
        }

        public List<SomeContract> Buffer = new List<SomeContract>();
        public List<TopicPartitionOffset> OffsetPartition = new List<TopicPartitionOffset>();
        private IMediator mediator;
        private XmlSerializer serializer;

        private bool MessageHandler(Confluent.Kafka.Message msg)
        {
            string s = Encoding.UTF8.GetString(msg.Value);
            using (MemoryStream memStream = new MemoryStream(msg.Value))
            using (StreamReader stream = new StreamReader(memStream, Encoding.UTF8))
            {
                var someContractObj = (SomeContract)serializer.Deserialize(stream);
                this.Buffer.Add(someContractObj);
                this.OffsetPartition.Add(msg.TopicPartitionOffset);
            }
            return false;
        }

        private async Task<bool> FlushHandler(FlushBufferMessage msg)
        {
            if (Buffer.Any())
            {
                await this.mediator.Send(new InsertOrUpdateSomeContract() { Data = Buffer });

                var coordinator = Context.System.ActorSelection("/user/akkaConsumerWrapper/messageRouter");

                coordinator.Tell(new BatchOffsetCommits() { Commits = OffsetPartition.Select(m => new CommitMessage() { TopicPartitionOffset = m }).ToList() });

                Buffer.Clear();

                this.OffsetPartition.Clear();
            }

            return false;
        }
    }
}