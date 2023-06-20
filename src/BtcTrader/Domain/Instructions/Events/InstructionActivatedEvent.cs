using MassTransit.Topology;

namespace BtcTrader.Domain.Instructions.Events;

public class InstructionActivatedEvent : IInstructionActivatedEvent
{
    public Guid InstructionId { get; set; }
    public Guid UserId { get; set; }
    public DateTime UpdatedTime { get; set; }
    public string CronExpression { get; set; }

}

[EntityName("BtcTraderExchange")]
public interface IInstructionActivatedEvent
{
    public Guid InstructionId { get; set; }
    public Guid UserId { get; set; }
    public DateTime UpdatedTime { get; set; }
    public string CronExpression { get; set; }
}