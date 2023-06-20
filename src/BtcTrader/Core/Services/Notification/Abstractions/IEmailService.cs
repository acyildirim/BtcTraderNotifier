namespace BtcTrader.Core.Services.Notification.Abstractions;

public interface IEmailService 
{
    Task Notify(string to, string subject, string html, string from = null);

}