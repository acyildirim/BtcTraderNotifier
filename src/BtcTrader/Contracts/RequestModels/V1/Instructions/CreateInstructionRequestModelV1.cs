using FluentValidation;
using FluentValidation.Results;

namespace BtcTrader.Contracts.RequestModels.V1.Instructions;

public class CreateInstructionRequestModelV1 
{
    public Guid UserId { get; set; }
    public string InstructionType { get; set; }
    public string CronExpression { get; set; }
    public double Amount { get; set; }
    public List<string>? NotificationChannel { get; set; }
    public DateTime InstructionDate { get; set; }
    public bool IsActive  = true;
    
    public ValidationResult ValidateRequest()
    {
        var validator = new CreateInstructionRequestModelValidator();
        return validator.Validate(this);
    }
}

public class CreateInstructionRequestModelValidator : AbstractValidator<CreateInstructionRequestModelV1>
{
    public CreateInstructionRequestModelValidator()
    {
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(100).LessThanOrEqualTo(20000)
            .WithMessage("Amount must be between 100 and 20000");
        RuleFor(x => x.InstructionType).NotNull();
        RuleFor(x => x.InstructionDate.Day).GreaterThanOrEqualTo(1).LessThanOrEqualTo(28)
            .WithMessage("You can only give an instructions within first 28 day of month");
        RuleFor(x => x.NotificationChannel).NotNull();
        RuleFor(x => x.UserId).NotNull();
        RuleFor(x => x.IsActive).NotEqual(false);
        RuleFor(x => x.CronExpression).NotNull();
    }
}