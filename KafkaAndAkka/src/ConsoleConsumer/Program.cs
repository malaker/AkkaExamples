using Akka.Actor;
using Akka.DI.AutoFac;
using Shared;
using Shared.IoC;
using System;

namespace ConsoleConsumer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var config = new AkkaSystemManagerConfig() { SystemHoconConfigPath = "configuration.hocon", SystemName = "mySystem" };
            AkkaSystemManager mgr = new AkkaSystemManager(config, (ActorSystem system) => new AutoFacDependencyResolver(AutofacContainer.Register(), system));
            mgr.RunActor<AkkaConsumerWrapper>("akkaConsumerWrapper");
            Console.ReadKey();
        }
    }
}