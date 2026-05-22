using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace MedControl.Api.Infrastructure.Email;

public sealed class SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    private readonly EmailOptions _options = options.Value;

    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.SmtpHost))
        {
            logger.LogInformation("Email SMTP nao configurado. Para: {To}. Assunto: {Subject}. Corpo: {Body}", to, subject, body);
            return;
        }

        using var message = new MailMessage(_options.From, to, subject, body)
        {
            IsBodyHtml = false
        };

        using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
        {
            EnableSsl = _options.UseSsl
        };

        if (!string.IsNullOrWhiteSpace(_options.Username))
        {
            client.Credentials = new NetworkCredential(_options.Username, _options.Password);
        }

        await client.SendMailAsync(message, cancellationToken);
    }
}
