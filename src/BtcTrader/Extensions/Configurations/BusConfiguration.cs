using BtcTrader.Domain.Instructions.Events;
using BtcTrader.Infrastructure.RabbitMq;
using MassTransit;
using MassTransit.RabbitMqTransport;
using RabbitMQ.Client;

namespace BtcTrader.Extensions.Configurations;

public static class BusConfigurator
{
    public static IBusControl Create(RabbitMqSettings appSettings)
    {
        return Bus.Factory.CreateUsingRabbitMq(factory =>
        {
            factory.Host("cluster", "/", h =>
            {
                h.Username(appSettings.UserName);
                h.Password(appSettings.Password);

                var rabbitCluster = appSettings.Uri.Split(";");

                h.UseCluster(c =>
                {
                    foreach (var server in rabbitCluster)
                        c.Node(server);
                });
            });

            ConfigureEventPublishConfiguration<IInstructionCreatedEvent>(factory);
            ConfigureEventSendConfiguration<IInstructionCreatedEvent>(factory,
                RoutingKeys.InstructionCreatedEventEventKey);
            ConfigureEventPublishConfiguration<IInstructionUpdatedEvent>(factory);
            ConfigureEventSendConfiguration<IInstructionUpdatedEvent>(factory,
                RoutingKeys.InstructionUpdatedEventEventKey); 
            ConfigureEventPublishConfiguration<IInstructionDeletedEvent>(factory);
            ConfigureEventSendConfiguration<IInstructionDeletedEvent>(factory,
                RoutingKeys.InstructionDeletedEventEventKey); 
            ConfigureEventPublishConfiguration<IInstructionActivatedEvent>(factory);
            ConfigureEventSendConfiguration<IInstructionActivatedEvent>(factory,
                RoutingKeys.InstructionActivatedEventEventKey);
            ConfigureEventPublishConfiguration<IInstructionDeActivatedEvent>(factory);
            ConfigureEventSendConfiguration<IInstructionDeActivatedEvent>(factory,
                RoutingKeys.InstructionDeActivatedEventEventKey);
            factory.UseRawJsonSerializer();
        });
    }

    private static void ConfigureEventPublishConfiguration<TEventType>(
        IRabbitMqBusFactoryConfigurator factoryConfigurator)
        where TEventType : class
    {
        factoryConfigurator.Publish<TEventType>(x => { x.ExchangeType = ExchangeType.Topic; });
    }

    private static void ConfigureEventSendConfiguration<TEventType>
        (IRabbitMqBusFactoryConfigurator factoryConfigurator, string routingKey)
        where TEventType : class
    {
        factoryConfigurator.Send<TEventType>(x => { x.UseRoutingKeyFormatter(context => routingKey); });
    }
}

