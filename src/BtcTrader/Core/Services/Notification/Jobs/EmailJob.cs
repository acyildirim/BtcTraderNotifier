using BtcTrader.Core.Services.Notification.Abstractions;

namespace BtcTrader.Core.Services.Notification.Jobs;


public class EmailJob
{
    private readonly ILogger<EmailJob> _logger;
    private readonly IEmailService _emailService;

    public EmailJob(ILogger<EmailJob> logger, IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }
    
    public async Task SendEmail(string to, string subject, string html, string from = null)
    {
        await _emailService.Notify(to,subject,html,from);
        _logger.LogInformation($"{subject} Mail sent successfully..,");
    }
    
}