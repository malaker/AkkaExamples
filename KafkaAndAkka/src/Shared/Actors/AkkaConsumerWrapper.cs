using Akka.Actor;
using Akka.Event;
using Shared.Interfaces;
using Shared.Messages;
using System.Linq;
using System.Threading;

namespace Shared
{
    public class AkkaConsumerWrapper : ReceiveActor
    {
        private IConsumerWrapper consumerWrapper;
        private CancellationTokenSource cts;
        private IConsumerWrapperFactory consumerWrapperFactory;
        private IActorRef localMachineMessageForwader;

        public AkkaConsumerWrapper(IConsumerWrapperFactory consumerWrapperFactory)
        {
            this.consumerWrapperFactory = consumerWrapperFactory;
            localMachineMessageForwader = Context.ActorOf(Props.Create<LocalMachineMessageRouter>(), "messageRouter");
            ConsumerWrapperInit();
        }

        private bool ConsumerWrapperInit()
        {
            this.consumerWrapper = consumerWrapperFactory.Create().WithMessageObserver(localMachineMessageForwader);
            this.cts = new CancellationTokenSource();
            this.consumerWrapper.Run(cts.Token);

            Become(Initialized);
            return false;
        }

        private void Initialized()
        {
            Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(1000*120, 4, Self, new PollMessageFromKafkaMessage(), Self);

            Receive<PollMessageFromKafkaMessage>(m =>
            {
                consumerWrapper.Poll();
            });

            Receive<AggregatedBatchesOfCommits>(async m =>
            {
                var topicpartof = m.Batches.SelectMany(l => l.Commits.Select(d => d.TopicPartitionOffset));

                await consumerWrapper.CommitMessageHandler(topicpartof);
            });

            Context.GetLogger().Info("Initialized") ;
        }
    }
}