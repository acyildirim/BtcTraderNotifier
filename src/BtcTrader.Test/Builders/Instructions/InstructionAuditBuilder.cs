using Bogus;
using BtcTrader.Domain.Instructions;
using BtcTrader.Infrastructure.RabbitMq;
using BtcTrader.Test.Common;

namespace BtcTrader.Test.Builders.Instructions;

public class InstructionAuditBuilder
{
    public static Guid id = Guid.NewGuid();
    public static Guid instructionId = Guid.NewGuid();
    public static Guid userId = Guid.NewGuid();
    public static Guid? megaMerchantId = Guid.NewGuid();
    public static double amount = double.MinValue;
    public static string instructionType = string.Empty;
    public static bool isActive = Generators.GenerateRandomBoolean();
    public static DateTime instructionDate = Generators.GenerateRandomDate();
    public static DateTime updatedTime = Generators.GenerateRandomDate();
    public string cronExpression = string.Empty;
    Random rnd = new Random();
    static List<string> notificationChannel = new()
    {
        "Email",
    };
    public static Dictionary<string, object>? createdDictionary = new Dictionary<string, object>
    {
        {"EventType", RabbitMqConstants.InstructionCreatedEvent},
        {"UserName", "UserName"},
        {"CreatedTime", Generators.GenerateRandomDate()}
    };

    public static Faker<InstructionAudit> MockInstructionAudit()
    {
        return new Faker<InstructionAudit>()
            .RuleFor(m => m.Id, id)
            .RuleFor(m => m.InstructionId, instructionId)
            .RuleFor(m => m.IsActive, isActive)
            .RuleFor(m => m.InstructionDate, instructionDate)
            .RuleFor(m => m.UpdatedTime, updatedTime)
            .RuleFor(m => m.InstructionType, faker => faker.Finance.Currency().Code)
            .RuleFor(m => m.CronExpression, faker => "*/20 * * * * *")
            .RuleFor(m => m.UserId, userId)
            .RuleFor(m => m.UserName, faker => faker.Name.FirstName())
            .RuleFor(m => m.NotificationChannel, faker => notificationChannel)
            .RuleFor(m => m.OldAmount, faker => 120)
            .RuleFor(m => m.NewAmount, faker => 150)
            .RuleFor(m => m.Message, "Message")
            .RuleFor(m => m.Details,createdDictionary);
    }
    
}