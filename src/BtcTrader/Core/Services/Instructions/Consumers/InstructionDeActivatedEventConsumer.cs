using AutoMapper;
using BtcTrader.Core.Services.Notification.Common;
using BtcTrader.Core.Services.Notification.Jobs;
using BtcTrader.Data.Repositories.Interfaces;
using BtcTrader.Domain.Instructions;
using BtcTrader.Domain.Instructions.Events;
using BtcTrader.Domain.User;
using BtcTrader.Domain.User.Exceptions;
using BtcTrader.Infrastructure.RabbitMq;
using Hangfire;
using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.Definition;
using MassTransit.RabbitMqTransport;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

namespace BtcTrader.Core.Services.Instructions.Consumers;

public class InstructionDeActivatedEventConsumer : IConsumer<InstructionDeActivatedEvent>
{
    private readonly IRepository<InstructionAudit> _instructionAuditRepository;
    private readonly ILogger<InstructionDeActivatedEventConsumer> _logger;
    private readonly UserManager<AppUser> _userManager;
    private readonly IMapper _mapper;
    
    public InstructionDeActivatedEventConsumer(
        ILogger<InstructionDeActivatedEventConsumer> logger,
        UserManager<AppUser> userManager, IMapper mapper, 
        IRepository<InstructionAudit> instructionAuditRepository)
    {
        _logger = logger;
        _userManager = userManager;
        _mapper = mapper;
        _instructionAuditRepository = instructionAuditRepository;
    }
    public async Task Consume(ConsumeContext<InstructionDeActivatedEvent> context)
    {
        if (context.Message == null) return;
        var instructionDeActivatedEvent = context.Message;
        if (!IsInstructionDeActivatedEventEligible(instructionDeActivatedEvent)) return;
        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == instructionDeActivatedEvent.UserId.ToString());
        if (user is null)
        {
            throw new UserNotFoundException($"User with related userId : {instructionDeActivatedEvent.UserId} could not found.");
        }
        await CreateAuditLog(instructionDeActivatedEvent,user);
        await CreateJob(instructionDeActivatedEvent,user);
    }
    

    private async Task CreateJob(InstructionDeActivatedEvent instructionDeActivatedEvent, AppUser user)
    {
        var to = user.Email;
        var subject = string.Format(Messages.INSTRUCTION_SUCCESSFULLY_DEACTIVATED_MESSAGE,
            instructionDeActivatedEvent.InstructionId,instructionDeActivatedEvent.UserId,user.UserName);
        var html = string.Format(Messages.INSTRUCTION_SUCCESSFULLY_DEACTIVATED_MESSAGE,
            instructionDeActivatedEvent.InstructionId,instructionDeActivatedEvent.UserId,user.UserName);
        
        var manager = new RecurringJobManager();
        var jobName = string.Concat(typeof(EmailJob).Name,"-",user.Id);
        manager.RemoveIfExists(jobName);
        _logger.LogInformation($"Instruction with {instructionDeActivatedEvent.InstructionId} successfully processed..");
    }

    private async Task CreateAuditLog(InstructionDeActivatedEvent instructionDeActivatedEvent, AppUser user)
    {
        var instructionAudit = new InstructionAudit();
        _mapper.Map(instructionDeActivatedEvent, instructionAudit);
        instructionAudit.UserName = user.UserName!;
        instructionAudit.Details = new Dictionary<string, object>
        {
            {"EventType", RabbitMqConstants.InstructionDeActivatedEvent},
            {"UserName", user.UserName},
            {"UpdatedTime", instructionDeActivatedEvent.UpdatedTime}
        };
        var isSuccess = await _instructionAuditRepository.InsertAsync(instructionAudit);
        if (!isSuccess)
        {
            _logger.LogInformation($"Error Occured While Creating {instructionAudit.GetType().FullName} with instructionId : {instructionAudit.InstructionId}");
        }
    }

    private bool IsInstructionDeActivatedEventEligible(InstructionDeActivatedEvent instructionDeActivatedEvent)
    {
        if (instructionDeActivatedEvent == null)
        {
            _logger.LogError(
                $"InstructionDeActivatedEvent is null.Payload : {instructionDeActivatedEvent}");
            return false;
        }

        return true;
    }

    
}

public class InstructionDeActivatedEventConsumerDefinition : ConsumerDefinition<InstructionDeActivatedEventConsumer>
{
    public InstructionDeActivatedEventConsumerDefinition()
    {
        EndpointName = RabbitMqConstants.InstructionDeActivatedEvent;
    }

    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<InstructionDeActivatedEventConsumer> consumerConfigurator)
    {
        if (endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rabbit)
        {
            rabbit.ConcurrentMessageLimit = 1;
            rabbit.Bind(RabbitMqExchanges.BtcTraderExchange, s =>
            {
                s.RoutingKey = RoutingKeys.InstructionDeActivatedEventEventKey;
                s.ExchangeType = ExchangeType.Topic;
            });
        }
        endpointConfigurator.ConfigureConsumeTopology = false;
        endpointConfigurator.ClearMessageDeserializers();
        endpointConfigurator.UseRawJsonSerializer();
    }
        
}