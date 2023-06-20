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

public class InstructionCreatedEventConsumer : IConsumer<InstructionCreatedEvent>
{
    private readonly IRepository<InstructionAudit> _instructionAuditRepository;
    private readonly ILogger<InstructionCreatedEventConsumer> _logger;
    private readonly UserManager<AppUser> _userManager;
    private readonly IMapper _mapper;

    public InstructionCreatedEventConsumer(
        ILogger<InstructionCreatedEventConsumer> logger,
        UserManager<AppUser> userManager, IMapper mapper, 
        IRepository<InstructionAudit> instructionAuditRepository)
    {
        _logger = logger;
        _userManager = userManager;
        _mapper = mapper;
        _instructionAuditRepository = instructionAuditRepository;
    }


    public async Task Consume(ConsumeContext<InstructionCreatedEvent> context)
    {
        if (context.Message == null) return;
        var instructionCreatedEvent = context.Message;
        if (!IsInstructionCreatedEventEligible(instructionCreatedEvent)) return;
        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == instructionCreatedEvent.UserId.ToString());
        if (user is null)
        {
            throw new UserNotFoundException($"User with related userId : {instructionCreatedEvent.UserId} could not found.");
        }
        await CreateAuditLog(instructionCreatedEvent,user);
        await CreateJob(instructionCreatedEvent,user);
    }

    private async Task CreateAuditLog(InstructionCreatedEvent instructionCreatedEvent,AppUser user)
    {
        var instructionAudit = new InstructionAudit();
        _mapper.Map(instructionCreatedEvent, instructionAudit);
        instructionAudit.UserName = user.UserName!;
        instructionAudit.Details = new Dictionary<string, object>
        {
            {"EventType", RabbitMqConstants.InstructionCreatedEvent},
            {"UserName", user.UserName},
            {"CreatedTime", instructionCreatedEvent.CreatedTime}
        };
        var isSuccess = await _instructionAuditRepository.InsertAsync(instructionAudit);
        if (!isSuccess)
        {
            _logger.LogInformation($"Error Occured While Creating {instructionAudit.GetType().FullName} with instructionId : {instructionAudit.InstructionId}");
        }
    }

    private Task CreateJob(InstructionCreatedEvent instructionCreatedEvent,AppUser user)
    {
        var to = user.Email;
        var subject = string.Format(Messages.INSTRUCTION_SUCCESSFULLY_CREATED_MESSAGE,
            instructionCreatedEvent.Amount,instructionCreatedEvent.InstructionDate,instructionCreatedEvent.InstructionType);
        var html = string.Format(Messages.INSTRUCTION_SUCCESSFULLY_CREATED_MESSAGE,
            instructionCreatedEvent.Amount,instructionCreatedEvent.InstructionDate,instructionCreatedEvent.InstructionType);

        var manager = new RecurringJobManager();
        var jobName = string.Concat(typeof(EmailJob).Name,"-",user.Id);
        manager.RemoveIfExists(jobName);
        manager.AddOrUpdate<EmailJob>(jobName, 
            emailJob => emailJob.SendEmail(to,subject,html,null),
            instructionCreatedEvent.CronExpression,
            TimeZoneInfo.Local);
        _logger.LogInformation($"Instruction with {instructionCreatedEvent.Id} successfully processed..");
        return Task.CompletedTask;
    }

    private bool IsInstructionCreatedEventEligible(InstructionCreatedEvent instructionCreatedEvent)
    {
        if (instructionCreatedEvent == null)
        {
            _logger.LogError(
                $"InstructionCreatedEvent is null.Payload : {instructionCreatedEvent}");
            return false;
        }

        return true;
    }
}

public class InstructionCreatedEventConsumerDefinition : ConsumerDefinition<InstructionCreatedEventConsumer>
{
    public InstructionCreatedEventConsumerDefinition()
    {
        EndpointName = RabbitMqConstants.InstructionCreatedEvent;
    }

    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<InstructionCreatedEventConsumer> consumerConfigurator)
    {
        if (endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rabbit)
        {
            rabbit.ConcurrentMessageLimit = 1;
            rabbit.Bind(RabbitMqExchanges.BtcTraderExchange, s =>
            {
                s.RoutingKey = RoutingKeys.InstructionCreatedEventEventKey;
                s.ExchangeType = ExchangeType.Topic;
            });
        }
        endpointConfigurator.ConfigureConsumeTopology = false;
        endpointConfigurator.ClearMessageDeserializers();
        endpointConfigurator.UseRawJsonSerializer();
    }
        
}