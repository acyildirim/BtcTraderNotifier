namespace BtcTrader.Infrastructure.Core;

public class SmtpSettings
{
    public string EmailFrom { get; set; }
    public string SmtpHost { get; set; }
    public string SmtpPort { get; set; }
    public string SmtpUser { get; set; }
    public string SmtpPass { get; set; }
}