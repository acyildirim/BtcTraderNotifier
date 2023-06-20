using MassTransit.Topology;

namespace BtcTrader.Domain.Instructions.Events;

public class InstructionDeActivatedEvent : IInstructionDeActivatedEvent
{
    public Guid InstructionId { get; set; }
    public Guid UserId { get; set; }
    public DateTime UpdatedTime { get; set; }
}

[EntityName("BtcTraderExchange")]
public interface IInstructionDeActivatedEvent
{
    public Guid InstructionId { get; set; }
    public Guid UserId { get; set; }
    public DateTime UpdatedTime { get; set; }
}