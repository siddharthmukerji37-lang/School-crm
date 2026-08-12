using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SchoolCRM.Application.Interfaces.Services;

namespace SchoolCRM.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        var host = _configuration["EmailSettings:Host"];
        if (string.IsNullOrWhiteSpace(host) ||
            string.IsNullOrWhiteSpace(_configuration["EmailSettings:FromEmail"]))
        {
            _logger.LogWarning("Email settings not configured. Skipping email to {To} about '{Subject}'.", to, subject);
            return;
        }

        if (string.IsNullOrWhiteSpace(to))
            return;

        try
        {
            var fromEmail = _configuration["EmailSettings:FromEmail"]!;
            var fromName = _configuration["EmailSettings:FromName"] ?? "School CRM";
            var port = int.TryParse(_configuration["EmailSettings:Port"], out var p) ? p : 587;
            var enableSsl = bool.TryParse(_configuration["EmailSettings:EnableSsl"], out var ssl) ? ssl : true;
            var username = _configuration["EmailSettings:Username"];
            var password = _configuration["EmailSettings:Password"];

            using var message = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(to);

#pragma warning disable SYSLIB0027
            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                UseDefaultCredentials = false,
                Credentials = !string.IsNullOrEmpty(username)
                    ? new NetworkCredential(username, password)
                    : null
            };
#pragma warning restore SYSLIB0027

            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent to {To} about '{Subject}'.", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To} about '{Subject}'.", to, subject);
        }
    }
}
