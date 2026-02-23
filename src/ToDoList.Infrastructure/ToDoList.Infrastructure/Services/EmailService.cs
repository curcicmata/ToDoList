using Microsoft.Extensions.Logging;
using ToDoList.Application.Interfaces;

namespace ToDoList.Infrastructure.Services;

public class EmailService(ILogger<EmailService> logger) : IEmailService
{
    public Task SendEmailAsync(string toAddress, string subject, string body)
    {
        logger.LogInformation(
            "[EMAIL] To: {ToAddress} | Subject: {Subject} | Body: {Body}",
            toAddress, subject, body);

        return Task.CompletedTask;
    }
}
