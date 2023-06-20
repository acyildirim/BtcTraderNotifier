namespace BtcTrader.Core.Services.Notification.Common;

public static class Messages
{
    public const string INSTRUCTION_SUCCESSFULLY_CREATED_MESSAGE =
        "Instruction with Amount : {0} Date :{1} Type : {2} is successfully created...";
    public const string INSTRUCTION_SUCCESSFULLY_UPDATED_MESSAGE =
        "Instruction with Amount : {0} Date :{1} Type : {2} is successfully updated by userId {3} and userName {4}...";
    public const string INSTRUCTION_SUCCESSFULLY_UPDATED_MESSAGE_WITH_AMOUNT =
        "Instruction with OldAmount : {0} , NewAmount : {1} Date :{2} Type : {3} is successfully updated by userId {4} and userName {5}...";
    public const string INSTRUCTION_SUCCESSFULLY_ACTIVATED_MESSAGE =
        "Instruction with id : {0} is successfully activated by userId : {1} userName : {2}...";
    public const string INSTRUCTION_SUCCESSFULLY_DEACTIVATED_MESSAGE =
        "Instruction with id : {0} is successfully deactivated by userId : {1} userName : {2}...";
    public const string INSTRUCTION_SUCCESSFULLY_DELETED_MESSAGE =
        "Instruction with id : {0} is successfully deleted by userId : {1} userName : {2}...";
}
