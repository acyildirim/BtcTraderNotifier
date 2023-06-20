using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BtcTrader.Domain.User;

namespace BtcTrader.Domain.Instructions;

public class Instruction 
{
    [Key] 
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; private set; }
    public Guid UserId { get; set; }
    
    public string InstructionType { get; set; }

    public double Amount { get; set; }

    public List<string> NotificationChannel { get; set; }

    public DateTime InstructionDate { get; set; }

    public bool IsActive { get; set; }
    public string CronExpression { get; set; }
    

    public Instruction()
    {
        Id = Guid.NewGuid();
        IsActive = true;
    }
    
}