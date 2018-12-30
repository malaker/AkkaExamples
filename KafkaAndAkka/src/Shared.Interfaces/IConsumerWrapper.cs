using Akka.Actor;
using Confluent.Kafka;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Shared.Interfaces
{
    public interface IConsumerWrapper
    {
        Task CommitMessageHandler(IEnumerable<TopicPartitionOffset> offsets);

        void Dispose();

        void Poll();

        IConsumerWrapper Run(CancellationToken token);

        IConsumerWrapper Subscribe(IEnumerable<string> topics);

        IConsumerWrapper WithMessageObserver(IActorRef observer);

        IConsumerWrapper WithPoolingTimeout(int timeout);
    }

    public interface IKnowActor<TActor> where TActor : ActorBase
    {
        IActorRef Ref { get; }
    }
}