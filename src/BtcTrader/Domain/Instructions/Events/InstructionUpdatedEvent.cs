using MassTransit;
using MassTransit.Topology;

namespace BtcTrader.Domain.Instructions.Events;

[EntityName("BtcTraderExchange")]
public interface IInstructionUpdatedEvent
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string InstructionType { get; set; }
    public double? OldAmount { get; set; }
    public double? NewAmount { get; set; }
    public List<string>? NotificationChannel { get; set; }
    public DateTime? InstructionDate { get; set; }
    public bool IsActive { get; set; }
    public string? CronExpression { get; set; }
    public DateTime UpdatedTime { get; set; }

}


public class InstructionUpdatedEvent : IInstructionUpdatedEvent
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string InstructionType { get; set; }
    public double? OldAmount { get; set; }
    public double? NewAmount { get; set; }
    public List<string>? NotificationChannel { get; set; }
    public DateTime? InstructionDate { get; set; }
    public bool IsActive { get; set; }
    public string? CronExpression { get; set; }
    public DateTime UpdatedTime { get; set; }
}