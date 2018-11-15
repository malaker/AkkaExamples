using Akka.Actor;
using Akka.Configuration;
using Akka.DI.Core;
using Shared.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shared
{
    public class AkkaSystemManager : IDisposable
    {
        private ActorSystem system;
        private Config config;
        private AkkaSystemManagerConfig systemConfig;
        private Func<ActorSystem, IDependencyResolver> diresolver;
        private Dictionary<string, IActorRef> actorsCreated = new Dictionary<string, IActorRef>();
        public Task WhenTerminated => system.WhenTerminated;

        public AkkaSystemManager(AkkaSystemManagerConfig config, Func<ActorSystem, IDependencyResolver> resolverCreator)
        {
            this.systemConfig = config;
            this.diresolver = resolverCreator;
        }

        public void Dispose()
        {
            system?.Dispose();
        }

        private void CreateActorSystem()
        {
            this.system = ActorSystem.Create(this.systemConfig.SystemName, config);
        }

        private void LoadConfig()
        {
            this.config = HoconLoader.ParseConfig(this.systemConfig.SystemHoconConfigPath);
        }

        public IActorRef RunActor<T>(string name) where T : ActorBase
        {
            if (config == null) LoadConfig();
            if (system == null) CreateActorSystem();

            IActorRef akkaConsumer = system.ActorOf(diresolver(system).Create<T>(), name);
            actorsCreated[name] = akkaConsumer;
            return akkaConsumer;
        }

        public Task Stop()
        {
            return CoordinatedShutdown.Get(system).Run();
        }
    }
}