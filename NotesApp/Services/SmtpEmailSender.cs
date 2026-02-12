using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Logging;
//send email using SMTP with MailKit, configured via SmtpOptions from appsettings.json
namespace NotesApp.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpOptions _options;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IOptions<SmtpOptions> options, ILogger<SmtpEmailSender> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var message = new MimeMessage();
                var fromEmail = string.IsNullOrWhiteSpace(_options.SenderEmail) ? _options.Username : _options.SenderEmail;
                message.From.Add(new MailboxAddress(_options.SenderName ?? fromEmail, fromEmail));
                message.To.Add(MailboxAddress.Parse(email));
                message.Subject = subject;
                message.Body = new BodyBuilder { HtmlBody = htmlMessage }.ToMessageBody();

                using var client = new SmtpClient();
                var secure = _options.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
                _logger.LogDebug("Connecting to SMTP {Host}:{Port} SSL={UseSsl}", _options.Host, _options.Port, _options.UseSsl);
                await client.ConnectAsync(_options.Host, _options.Port, secure);

                if (!string.IsNullOrEmpty(_options.Username))
                {
                    _logger.LogDebug("Authenticating SMTP as {Username}", _options.Username);
                    await client.AuthenticateAsync(_options.Username, _options.Password ?? string.Empty);
                }

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Sent email to {Recipient}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed sending email to {Recipient}", email);
                throw;
            }
        }
    }
}