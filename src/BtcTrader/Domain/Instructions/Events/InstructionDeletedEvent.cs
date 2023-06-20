using MassTransit.Topology;

namespace BtcTrader.Domain.Instructions.Events;

public class InstructionDeletedEvent : IInstructionDeletedEvent
{
    public Guid InstructionId { get; set; }
    public Guid UserId { get; set; }
    public DateTime UpdatedTime { get; set; }
}


[EntityName("BtcTraderExchange")]
public interface IInstructionDeletedEvent
{
    public Guid InstructionId { get; set; }
    public Guid UserId { get; set; }
    public DateTime UpdatedTime { get; set; }

}