using Bogus;
using BtcTrader.Domain.Instructions;
using BtcTrader.Test.Common;

namespace BtcTrader.Test.Builders.Instructions;

public class InstructionBuilder
{
    public static Guid id = Guid.NewGuid();
    public static Guid userId = Guid.NewGuid();
    public static Guid? megaMerchantId = Guid.NewGuid();
    public static double amount = double.MinValue;
    public static string instructionType = string.Empty;
    public static bool isActive = Generators.GenerateRandomBoolean();
    public static DateTime instructionDate = Generators.GenerateRandomDate();
    public string cronExpression = string.Empty;
    Random rnd = new Random();
    static List<string> notificationChannel = new()
    {
        "Email",
    };



    public static Faker<Instruction> MockInstruction()
    {
        return new Faker<Instruction>()
            .RuleFor(m => m.Id, id)
            .RuleFor(m => m.IsActive, isActive)
            .RuleFor(m => m.InstructionDate, instructionDate)
            .RuleFor(m => m.InstructionType, faker => faker.Finance.Currency().Code)
            .RuleFor(m => m.CronExpression, faker => "*/20 * * * * *")
            .RuleFor(m => m.UserId, userId)
            .RuleFor(m => m.NotificationChannel, faker => notificationChannel)
            .RuleFor(m => m.Amount, faker => 120);
    }
    
}