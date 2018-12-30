﻿using Autofac;
using MediatR;
using MediatR.Pipeline;
using System.Reflection;

namespace Shared.IoC
{
    public static class AutofacContainer
    {
        public static IContainer Register(Akka.Configuration.Config config)
        {
            var builder = new ContainerBuilder();
            builder.Register<Akka.Configuration.Config>(c => config).AsSelf();
            //builder.RegisterType<ConsumerWrapper>().AsSelf().AsImplementedInterfaces();
            //builder.RegisterType<FakeConsumerFactory>().AsImplementedInterfaces();
            builder.RegisterType<KafkaConsumerConfig>().AsSelf().AsImplementedInterfaces();
            builder.RegisterType<AkkaConsumerWrapper>().AsSelf().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ConsumerWrapperFactory>().AsSelf().AsImplementedInterfaces();
            builder.RegisterType<SqlConnectionProvider>().AsSelf().AsImplementedInterfaces(); ;
            builder.RegisterType<SimpleAkkaMessageProcessor>().AsSelf().AsImplementedInterfaces();
            builder.RegisterAssemblyTypes(typeof(IMediator).GetTypeInfo().Assembly).AsImplementedInterfaces();

            var mediatrOpenTypes = new[]
          {
                typeof(IRequestHandler<,>),
                typeof(INotificationHandler<>),
            };

            foreach (var mediatrOpenType in mediatrOpenTypes)
            {
                builder
                    .RegisterAssemblyTypes(typeof(InsertOrUpdateSomeContract).GetTypeInfo().Assembly)
                    .AsClosedTypesOf(mediatrOpenType)
                    .AsImplementedInterfaces();
            }

            builder.RegisterGeneric(typeof(RequestPostProcessorBehavior<,>)).As(typeof(IPipelineBehavior<,>));
            builder.RegisterGeneric(typeof(RequestPreProcessorBehavior<,>)).As(typeof(IPipelineBehavior<,>));
            builder.Register<ServiceFactory>(ctx =>
            {
                var c = ctx.Resolve<IComponentContext>();
                return t => c.Resolve(t);
            });

            return builder.Build();
        }
    }
}