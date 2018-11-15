using Akka.Actor;
using Akka.DI.AutoFac;
using Serilog;
using Shared;
using Shared.IoC;
using System;

namespace ConsoleConsumer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var logger = new LoggerConfiguration()
                        .WriteTo.Console()
                        .MinimumLevel.Information()
                        .CreateLogger();
            Serilog.Log.Logger = logger;

            var config = new AkkaSystemManagerConfig() { SystemHoconConfigPath = "configuration.hocon", SystemName = "mySystem" };
            AkkaSystemManager mgr = new AkkaSystemManager(config, (ActorSystem system) => new AutoFacDependencyResolver(AutofacContainer.Register(), system));
            Console.CancelKeyPress += (sender, eventArgs) => { mgr.Stop(); };
            mgr.RunActor<AkkaConsumerWrapper>("akkaConsumerWrapper");
            mgr.WhenTerminated.Wait();
            Console.WriteLine("Progam is exiting");
        }
    }
}