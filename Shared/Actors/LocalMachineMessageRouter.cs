using Akka.Actor;
using Akka.DI.Core;
using Akka.Routing;
using Confluent.Kafka;
using Shared.Messages;
using System.Collections.Generic;

namespace Shared
{
    public class LocalMachineMessageRouter : ReceiveActor
    {
        private IActorRef messageProcessorActorRouter;
        private List<BatchOffsetCommits> batches = new List<BatchOffsetCommits>();

        public LocalMachineMessageRouter()
        {
            this.messageProcessorActorRouter = Context.ActorOf(Context.System.DI().Props<SimpleAkkaMessageProcessor>().WithRouter(FromConfig.Instance), "messageForwarder");
            Receive<Message<Null, string>>(Forward);
            Receive<BatchOffsetCommits>(PushToBuffer);
            Receive<FlushBufferMessage>(Flush);
            Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(1000, 2000, Self, new FlushBufferMessage(), Self);
        }

        public bool Forward(Message<Null, string> value)
        {
            this.messageProcessorActorRouter.Tell(value);
            return false;
        }

        public bool PushToBuffer(BatchOffsetCommits batchCommits)
        {
            this.batches.Add(batchCommits);
            return false;
        }

        public bool Flush(FlushBufferMessage m)
        {
            var aggregate = new AggregatedBatchesOfCommits() { Batches = this.batches };
            var coordinator = Context.System.ActorSelection("/user/akkaConsumerWrapper");
            coordinator.Tell(aggregate);
            this.batches.Clear();
            return false;
        }
    }
}