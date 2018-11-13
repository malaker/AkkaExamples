using Akka.Actor;
using Akka.DI.AutoFac;
using Shared;
using Shared.IoC;
using System;

namespace KafkaConsumer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            AkkaSystemManager mgr = new AkkaSystemManager("configuration.hocon", "mySystem", (ActorSystem system) => new AutoFacDependencyResolver(AutofacContainer.Register(), system));
            mgr.RunActor<AkkaConsumerWrapper>("akkaConsumerWrapper");
            Console.ReadKey();
        }
    }
}