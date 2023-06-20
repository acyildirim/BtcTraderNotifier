using FluentValidation;
using FluentValidation.Results;

namespace BtcTrader.Contracts.RequestModels.V1.Instructions;

public class UpdateInstructionRequestModelV1
{
    public Guid UserId { get; set; }
    public double? Amount { get; set; }
    public List<string>? NotificationChannel { get; set; }
    public DateTime? InstructionDate { get; set; }
    public string? CronExpression { get; set; }

    
    public ValidationResult ValidateRequest()
    {
        var validator = new UpdateInstructionRequestModelV1Validator();
        return validator.Validate(this);
    }
}

public class UpdateInstructionRequestModelV1Validator : AbstractValidator<UpdateInstructionRequestModelV1>
{
    public UpdateInstructionRequestModelV1Validator()
    {
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(100).LessThanOrEqualTo(20000)
            .WithMessage("Amount must be between 100 and 20000");
        RuleFor(x => x.InstructionDate.Value.Day)
            .GreaterThanOrEqualTo(1).LessThanOrEqualTo(28)
            .When(x=>x.InstructionDate is not null)
            .WithMessage("You can only give an instructions within first 28 day of month");
    }
}