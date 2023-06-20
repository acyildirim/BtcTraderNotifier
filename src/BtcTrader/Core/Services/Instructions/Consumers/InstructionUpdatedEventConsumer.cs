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

public class InstructionUpdatedEventConsumer : IConsumer<InstructionUpdatedEvent>
{
    private readonly IRepository<InstructionAudit> _instructionAuditRepository;
    private readonly ILogger<InstructionUpdatedEventConsumer> _logger;
    private readonly UserManager<AppUser> _userManager;
    private readonly IMapper _mapper;
    
    public InstructionUpdatedEventConsumer(
        ILogger<InstructionUpdatedEventConsumer> logger,
        UserManager<AppUser> userManager, IMapper mapper, 
        IRepository<InstructionAudit> instructionAuditRepository)
    {
        _logger = logger;
        _userManager = userManager;
        _mapper = mapper;
        _instructionAuditRepository = instructionAuditRepository;
    }

    public async Task Consume(ConsumeContext<InstructionUpdatedEvent> context)
    {
        if (context.Message == null) return;
        var instructionUpdatedEvent = context.Message;
        if (!IsInstructionUpdatedEventEligible(instructionUpdatedEvent)) return;
        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == instructionUpdatedEvent.UserId.ToString());
        if (user is null)
        {
            throw new UserNotFoundException($"User with related userId : {instructionUpdatedEvent.UserId} could not found.");
        }
        await CreateAuditLog(instructionUpdatedEvent,user);
        await CreateJob(instructionUpdatedEvent,user);
    }

    private async Task CreateJob(InstructionUpdatedEvent instructionUpdatedEvent, AppUser user)
    {
        var to = user.Email;
        var subject = string.Format(Messages.INSTRUCTION_SUCCESSFULLY_UPDATED_MESSAGE,
            instructionUpdatedEvent.OldAmount,instructionUpdatedEvent.NewAmount,
            instructionUpdatedEvent.InstructionDate,instructionUpdatedEvent.InstructionType,
            instructionUpdatedEvent.UserId,user.UserName);
        var html = string.Format(Messages.INSTRUCTION_SUCCESSFULLY_UPDATED_MESSAGE,
            instructionUpdatedEvent.OldAmount,
            instructionUpdatedEvent.InstructionDate,instructionUpdatedEvent.InstructionType,
            instructionUpdatedEvent.UserId,user.UserName);
        if (instructionUpdatedEvent.NewAmount != null)
        {
            subject =string.Format(Messages.INSTRUCTION_SUCCESSFULLY_UPDATED_MESSAGE_WITH_AMOUNT,
                instructionUpdatedEvent.OldAmount,instructionUpdatedEvent.NewAmount,
                instructionUpdatedEvent.InstructionDate,instructionUpdatedEvent.InstructionType,
                instructionUpdatedEvent.UserId,user.UserName);
            html = string.Format(Messages.INSTRUCTION_SUCCESSFULLY_UPDATED_MESSAGE_WITH_AMOUNT,
                instructionUpdatedEvent.OldAmount,instructionUpdatedEvent.NewAmount,
                instructionUpdatedEvent.InstructionDate,instructionUpdatedEvent.InstructionType,
                instructionUpdatedEvent.UserId,user.UserName);
        }
        var manager = new RecurringJobManager();
        var jobName = string.Concat(typeof(EmailJob).Name,"-",user.Id);
        manager.RemoveIfExists(jobName);
        manager.AddOrUpdate<EmailJob>(jobName, 
            emailJob => emailJob.SendEmail(to,subject,html,null),
            instructionUpdatedEvent.CronExpression,
            TimeZoneInfo.Local);
        _logger.LogInformation($"Instruction with {instructionUpdatedEvent.Id} successfully processed..");
    }

    private async Task CreateAuditLog(InstructionUpdatedEvent instructionUpdatedEvent, AppUser user)
    {
        var instructionAudit = new InstructionAudit();
        _mapper.Map(instructionUpdatedEvent, instructionAudit);
        instructionAudit.UserName = user.UserName!;
        instructionAudit.Details = new Dictionary<string, object>
        {
            {"EventType", RabbitMqConstants.InstructionUpdatedEvent},
            {"UserName", user.UserName},
            {"UpdatedTime", instructionUpdatedEvent.UpdatedTime}
        };
        var isSuccess = await _instructionAuditRepository.InsertAsync(instructionAudit);
        if (!isSuccess)
        {
            _logger.LogInformation($"Error Occured While Creating {instructionAudit.GetType().FullName} with instructionId : {instructionAudit.InstructionId}");
        }
    }

    private bool IsInstructionUpdatedEventEligible(InstructionUpdatedEvent instructionUpdatedEvent)
    {
        if (instructionUpdatedEvent == null)
        {
            _logger.LogError(
                $"InstructionUpdatedEvent is null.Payload : {instructionUpdatedEvent}");
            return false;
        }

        return true;
    }
}

public class InstructionUpdatedEventConsumerDefinition : ConsumerDefinition<InstructionUpdatedEventConsumer>
{
    public InstructionUpdatedEventConsumerDefinition()
    {
        EndpointName = RabbitMqConstants.InstructionUpdatedEvent;
    }

    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<InstructionUpdatedEventConsumer> consumerConfigurator)
    {
        if (endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rabbit)
        {
            rabbit.ConcurrentMessageLimit = 1;
            rabbit.Bind(RabbitMqExchanges.BtcTraderExchange, s =>
            {
                s.RoutingKey = RoutingKeys.InstructionUpdatedEventEventKey;
                s.ExchangeType = ExchangeType.Topic;
            });
        }
        endpointConfigurator.ConfigureConsumeTopology = false;
        endpointConfigurator.ClearMessageDeserializers();
        endpointConfigurator.UseRawJsonSerializer();
    }
        
}