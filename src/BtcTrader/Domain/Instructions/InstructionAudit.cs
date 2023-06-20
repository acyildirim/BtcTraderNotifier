using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace BtcTrader.Domain.Instructions;

public class InstructionAudit
{
    [Key] 
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }
    public Guid InstructionId { get;set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string? InstructionType { get; set; }
    public List<string>? NotificationChannel { get; set; }
    public DateTime? InstructionDate { get; set; }
    public bool? IsActive { get; set; }
    public string? CronExpression { get; set; }
    public DateTime? UpdatedTime { get; set; }
    public double? OldAmount{ get; set; }
    public double? NewAmount { get; set; }
    public string? Message { get; set; }
    [JsonExtensionData]
    [Column(TypeName = "jsonb")]
    public Dictionary<string, object>? Details { get;  set; }

    public InstructionAudit()
    {
        Id = Guid.NewGuid();
        IsActive = true;
    }
}