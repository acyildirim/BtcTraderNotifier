namespace BtcTrader.Core.Services.Instructions.Common;

public static class EventAuditMessages
{
    public const string INSTRUCTION_CREATED_BY_USER =
        "Instruction : {0} is created by UserId : {1} at time {2}";
    public const string INSTRUCTION_UPDATED_BY_USER =
        "Instruction : {0} is updated by UserId : {1} at time {2}";
    public const string INSTRUCTION_ACTIVATED_BY_USER =
        "Instruction : {0} is activated by UserId : {1} at time {2}";
    public const string INSTRUCTION_DEACTIVATED_BY_USER =
        "Instruction : {0} is deactivated by UserId : {1} at time {2}";
    public const string INSTRUCTION_DELETED_BY_USER =
        "Instruction : {0} is deleted by UserId : {1} at time {2}";
}