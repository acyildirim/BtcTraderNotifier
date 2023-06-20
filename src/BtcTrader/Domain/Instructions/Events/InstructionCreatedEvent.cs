using MassTransit;
using MassTransit.Topology;

namespace BtcTrader.Domain.Instructions.Events;

[EntityName("BtcTraderExchange")]
public interface IInstructionCreatedEvent
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string InstructionType { get; set; }
    public double Amount { get; set; }
    public List<string> NotificationChannel { get; set; }
    public DateTime InstructionDate { get; set; }
    public bool IsActive { get; set; }
    public string CronExpression { get; set; }
    public DateTime CreatedTime { get; set; }

}


public class InstructionCreatedEvent : IInstructionCreatedEvent
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string InstructionType { get; set; }
    public double Amount { get; set; }
    public List<string> NotificationChannel { get; set; }
    public DateTime InstructionDate { get; set; }
    public bool IsActive { get; set; }
    public string CronExpression { get; set; }
    public DateTime CreatedTime { get; set; }
}