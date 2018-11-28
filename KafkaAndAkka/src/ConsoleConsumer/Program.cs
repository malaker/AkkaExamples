using Akka.Actor;
using Akka.DI.AutoFac;
using Serilog;
using Shared;
using Shared.IoC;
using System;
using System.Net.NetworkInformation;

namespace ConsoleConsumer
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
            PingHost("mssql", true);
            var logger = new LoggerConfiguration()
                        .WriteTo.Console()
                        .MinimumLevel.Information()
                        .CreateLogger();
            Serilog.Log.Logger = logger;

            var config = new AkkaSystemManagerConfig() { SystemHoconConfigPath = "configuration.hocon", SystemName = "mySystem" };
            AkkaSystemManager mgr = new AkkaSystemManager(config, (ActorSystem system, Akka.Configuration.Config c) => new AutoFacDependencyResolver(AutofacContainer.Register(c), system));
            Console.CancelKeyPress += (sender, eventArgs) => { mgr.Stop(); };
            mgr.RunActor<AkkaConsumerWrapper>("akkaConsumerWrapper");
            mgr.WhenTerminated.Wait();
            Console.WriteLine("Progam is exiting");
        }
    }
}