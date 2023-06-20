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

public class InstructionActivatedEventConsumer : IConsumer<InstructionActivatedEvent>
{
    private readonly ILogger<InstructionActivatedEventConsumer> _logger;
    private readonly IRepository<InstructionAudit> _instructionAuditRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly IMapper _mapper;
    
    public InstructionActivatedEventConsumer(
        ILogger<InstructionActivatedEventConsumer> logger,
        UserManager<AppUser> userManager, IMapper mapper,
        IRepository<InstructionAudit> instructionAuditRepository)
    {
        _logger = logger;
        _userManager = userManager;
        _mapper = mapper;
        _instructionAuditRepository = instructionAuditRepository;
    }
    public async Task Consume(ConsumeContext<InstructionActivatedEvent> context)
    {
        if (context.Message == null) return;
        var instructionActivatedEvent = context.Message;
        if (!IsInstructionActivatedEventEligible(instructionActivatedEvent)) return;
        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == instructionActivatedEvent.UserId.ToString());
        if (user is null)
        {
            throw new UserNotFoundException($"User with related userId : {instructionActivatedEvent.UserId} could not found.");
        }
        await CreateAuditLog(instructionActivatedEvent,user);
        await CreateJob(instructionActivatedEvent,user);
    }
    

    private async Task CreateJob(InstructionActivatedEvent instructionActivatedEvent, AppUser user)
    {
        var to = user.Email;
        var subject = string.Format(Messages.INSTRUCTION_SUCCESSFULLY_ACTIVATED_MESSAGE,
            instructionActivatedEvent.InstructionId,instructionActivatedEvent.UserId,user.UserName);
        var html = string.Format(Messages.INSTRUCTION_SUCCESSFULLY_ACTIVATED_MESSAGE,
            instructionActivatedEvent.InstructionId,instructionActivatedEvent.UserId,user.UserName);
        
        var manager = new RecurringJobManager();
        var jobName = string.Concat(typeof(EmailJob).Name,"-",user.Id);
        manager.RemoveIfExists(jobName);
        manager.AddOrUpdate<EmailJob>(jobName, 
            emailJob => emailJob.SendEmail(to,subject,html,null),
            instructionActivatedEvent.CronExpression,
            TimeZoneInfo.Local);
        _logger.LogInformation($"Instruction with {instructionActivatedEvent.InstructionId} successfully processed..");
    }

    private async Task CreateAuditLog(InstructionActivatedEvent instructionActivatedEvent, AppUser user)
    {
        var instructionAudit = new InstructionAudit();
        _mapper.Map(instructionActivatedEvent, instructionAudit);
        instructionAudit.UserName = user.UserName!;
        instructionAudit.Details = new Dictionary<string, object>
        {
            {"EventType", RabbitMqConstants.InstructionActivatedEvent},
            {"UserName", user.UserName},
            {"UpdatedTime", instructionActivatedEvent.UpdatedTime}
        };
        var isSuccess = await _instructionAuditRepository.InsertAsync(instructionAudit);
        if (!isSuccess)
        {
            _logger.LogInformation($"Error Occured While Creating {instructionAudit.GetType().FullName} with instructionId : {instructionAudit.InstructionId}");
        }
    }

    private bool IsInstructionActivatedEventEligible(InstructionActivatedEvent instructionActivatedEvent)
    {
        if (instructionActivatedEvent == null)
        {
            _logger.LogError(
                $"InstructionActivatedEvent is null.Payload : {instructionActivatedEvent}");
            return false;
        }

        return true;
    }

    
}

public class InstructionActivatedEventConsumerDefinition : ConsumerDefinition<InstructionActivatedEventConsumer>
{
    public InstructionActivatedEventConsumerDefinition()
    {
        EndpointName = RabbitMqConstants.InstructionActivatedEvent;
    }

    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<InstructionActivatedEventConsumer> consumerConfigurator)
    {
        if (endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rabbit)
        {
            rabbit.ConcurrentMessageLimit = 1;
            rabbit.Bind(RabbitMqExchanges.BtcTraderExchange, s =>
            {
                s.RoutingKey = RoutingKeys.InstructionActivatedEventEventKey;
                s.ExchangeType = ExchangeType.Topic;
            });
        }
        endpointConfigurator.ConfigureConsumeTopology = false;
        endpointConfigurator.ClearMessageDeserializers();
        endpointConfigurator.UseRawJsonSerializer();
    }
        
}