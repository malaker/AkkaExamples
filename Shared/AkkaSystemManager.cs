using Akka.Actor;
using Akka.Configuration;
using Akka.DI.Core;
using Shared.Common;
using System;
using System.Collections.Generic;

namespace Shared
{
    public class AkkaSystemManager : IDisposable
    {
        private ActorSystem system;
        private Config config;
        private string configPath;
        private string actorSystemName;
        private Func<ActorSystem, IDependencyResolver> diresolver;
        private Dictionary<string, IActorRef> actorsCreated = new Dictionary<string, IActorRef>();

        public AkkaSystemManager(string configPath, string actorsystemName, Func<ActorSystem, IDependencyResolver> resolverCreator)
        {
            this.configPath = configPath;
            this.actorSystemName = actorsystemName;
            this.diresolver = resolverCreator;
        }

        public void Dispose()
        {
            system?.Dispose();
        }

        private void CreateActorSystem()
        {
            this.system = ActorSystem.Create("KafkaConsumingSystem", config);
        }

        private void LoadConfig()
        {
            this.config = HoconLoader.ParseConfig(configPath);
        }

        public IActorRef RunActor<T>(string name) where T : ActorBase
        {
            if (config == null) LoadConfig();
            if (system == null) CreateActorSystem();

            IActorRef akkaConsumer = system.ActorOf(diresolver(system).Create<T>(), name);
            actorsCreated[name] = akkaConsumer;
            return akkaConsumer;
        }
    }
}