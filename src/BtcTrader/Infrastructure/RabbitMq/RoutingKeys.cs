namespace BtcTrader.Infrastructure.RabbitMq;

public static class RoutingKeys
{
    public const string InstructionCreatedEventEventKey = "InstructionService.Event.Instruction.V1.InstructionCreatedEvent";
    public const string InstructionUpdatedEventEventKey = "InstructionService.Event.Instruction.V1.InstructionUpdateEventEventKey";
    public const string InstructionDeletedEventEventKey = "InstructionService.Event.Instruction.V1.InstructionDeletedEventEventKey";
    public const string InstructionActivatedEventEventKey = "InstructionService.Event.Instruction.V1.InstructionActivatedEventEventKey";
    public const string InstructionDeActivatedEventEventKey = "InstructionService.Event.Instruction.V1.InstructionDeActivatedEventEventKey";
}