using BtcTrader.Core.Services.Instructions.Consumers;
using BtcTrader.Infrastructure.RabbitMq;
using MassTransit;

namespace BtcTrader.Extensions.Configurations;

public static class MassTransitConfiguration
{
    public static IServiceCollection AddMassTransitServices(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMqSettings = configuration.GetSection("RabbitMqSettings").Get<RabbitMqSettings>();

        services.AddMassTransit(x =>
        {
            
            x.AddConsumer<InstructionCreatedEventConsumer>(typeof(InstructionCreatedEventConsumerDefinition));
            x.AddConsumer<InstructionUpdatedEventConsumer>(typeof(InstructionUpdatedEventConsumerDefinition));
            x.AddConsumer<InstructionActivatedEventConsumer>(typeof(InstructionActivatedEventConsumerDefinition));
            x.AddConsumer<InstructionDeActivatedEventConsumer>(typeof(InstructionDeActivatedEventConsumerDefinition));
            x.AddConsumer<InstructionDeletedEventConsumer>(typeof(InstructionDeletedEventConsumerDefinition));
            x.UsingRabbitMq((context, cfg) =>
            {
                
                cfg.Host("cluster", "/", h =>
                {
                    h.Username(rabbitMqSettings.UserName);
                    h.Password(rabbitMqSettings.Password);
        
                    var rabbitCluster = rabbitMqSettings.Uri.Split(";");
        
                    h.UseCluster(c =>
                    {
                        foreach (var server in rabbitCluster)
                            c.Node(server);
                    });
                });
                cfg.ConfigureEndpoints(context);
            });
            
        });
        services.AddMassTransitHostedService();

        services.AddSingleton<IBus>(sp => BusConfigurator.Create(rabbitMqSettings));

        return services;
    }
      
}