using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BtcTrader.Domain.Notifications;

public class Notification
{
    [Key] 
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; private set; }
    public List<string> NotificationChannel { get; set; }
    public Guid UserId { get; set; }


    public DateTime InstructionDate { get; set; }

    public bool IsActive { get; set; }

    public Notification()
    {
        Id = Guid.NewGuid();
        IsActive = true;
    }
}
