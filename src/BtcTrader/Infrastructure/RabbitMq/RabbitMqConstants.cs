namespace BtcTrader.Infrastructure.RabbitMq;

public class RabbitMqConstants
{
    public const string InstructionCreatedEvent = "In.Instruction.InstructionCreatedEvent";
    public const string InstructionUpdatedEvent = "In.Instruction.InstructionUpdatedEvent";
    public const string InstructionActivatedEvent = "In.Instruction.InstructionActivatedEvent";
    public const string InstructionDeActivatedEvent = "In.Instruction.InstructionDeActivatedEvent";
    public const string InstructionDeletedEvent = "In.Instruction.InstructionDeletedEvent";
}