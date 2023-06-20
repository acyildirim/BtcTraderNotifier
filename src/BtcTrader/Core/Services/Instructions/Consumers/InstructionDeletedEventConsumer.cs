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

public class InstructionDeletedEventConsumer : IConsumer<InstructionDeletedEvent>
{
    private readonly IRepository<InstructionAudit> _instructionAuditRepository;
    private readonly ILogger<InstructionDeletedEventConsumer> _logger;
    private readonly UserManager<AppUser> _userManager;
    private readonly IMapper _mapper;
    
    public InstructionDeletedEventConsumer(
        ILogger<InstructionDeletedEventConsumer> logger,
        UserManager<AppUser> userManager, IMapper mapper, 
        IRepository<InstructionAudit> instructionAuditRepository)
    {
        _logger = logger;
        _userManager = userManager;
        _mapper = mapper;
        _instructionAuditRepository = instructionAuditRepository;
    }
    public async Task Consume(ConsumeContext<InstructionDeletedEvent> context)
    {
        if (context.Message == null) return;
        var instructionDeletedEvent = context.Message;
        if (!IsInstructionDeletedEventEligible(instructionDeletedEvent)) return;
        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == instructionDeletedEvent.UserId.ToString());
        if (user is null)
        {
            throw new UserNotFoundException($"User with related userId : {instructionDeletedEvent.UserId} could not found.");
        }
        await CreateAuditLog(instructionDeletedEvent,user);
        await CreateJob(instructionDeletedEvent,user);
    }
    

    private async Task CreateJob(InstructionDeletedEvent instructionDeletedEvent, AppUser user)
    {
        var to = user.Email;
        var subject = string.Format(Messages.INSTRUCTION_SUCCESSFULLY_DELETED_MESSAGE,
            instructionDeletedEvent.InstructionId,instructionDeletedEvent.UserId,user.UserName);
        var html = string.Format(Messages.INSTRUCTION_SUCCESSFULLY_DELETED_MESSAGE,
            instructionDeletedEvent.InstructionId,instructionDeletedEvent.UserId,user.UserName);
        
        var manager = new RecurringJobManager();
        var jobName = string.Concat(typeof(EmailJob).Name,"-",user.Id);
        manager.RemoveIfExists(jobName);
        _logger.LogInformation($"Instruction with {instructionDeletedEvent.InstructionId} successfully processed..");
    }

    private async Task CreateAuditLog(InstructionDeletedEvent instructionDeletedEvent, AppUser user)
    {
        var instructionAudit = new InstructionAudit();
        _mapper.Map(instructionDeletedEvent, instructionAudit);
        instructionAudit.UserName = user.UserName!;
        instructionAudit.Details = new Dictionary<string, object>
        {
            {"EventType", RabbitMqConstants.InstructionDeletedEvent},
            {"UserName", user.UserName},
            {"DeletedTime", instructionDeletedEvent.UpdatedTime}
        };
        var isSuccess = await _instructionAuditRepository.InsertAsync(instructionAudit);
        if (!isSuccess)
        {
            _logger.LogInformation($"Error Occured While Creating {instructionAudit.GetType().FullName} with instructionId : {instructionAudit.InstructionId}");
        }
    }

    private bool IsInstructionDeletedEventEligible(InstructionDeletedEvent instructionDeletedEvent)
    {
        if (instructionDeletedEvent == null)
        {
            _logger.LogError(
                $"InstructionDeletedEvent is null.Payload : {instructionDeletedEvent}");
            return false;
        }

        return true;
    }

    
}

public class InstructionDeletedEventConsumerDefinition : ConsumerDefinition<InstructionDeletedEventConsumer>
{
    public InstructionDeletedEventConsumerDefinition()
    {
        EndpointName = RabbitMqConstants.InstructionDeletedEvent;
    }

    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<InstructionDeletedEventConsumer> consumerConfigurator)
    {
        if (endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rabbit)
        {
            rabbit.ConcurrentMessageLimit = 1;
            rabbit.Bind(RabbitMqExchanges.BtcTraderExchange, s =>
            {
                s.RoutingKey = RoutingKeys.InstructionDeletedEventEventKey;
                s.ExchangeType = ExchangeType.Topic;
            });
        }
        endpointConfigurator.ConfigureConsumeTopology = false;
        endpointConfigurator.ClearMessageDeserializers();
        endpointConfigurator.UseRawJsonSerializer();
    }
        
}