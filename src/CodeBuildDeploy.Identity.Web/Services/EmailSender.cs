using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;

using Azure;
using Azure.Communication.Email;

namespace CodeBuildDeploy.Identity.Web.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly string? _connectionString;
        private readonly string? _senderEmailAddress;

        public EmailSender(IConfiguration configuration)
        {
            var emailSettingsSection = configuration.GetSection("EmailSettings");
            if (emailSettingsSection.Exists())
            {
                Configured = true;
                _connectionString = emailSettingsSection["ConnectionString"]!;
                _senderEmailAddress = emailSettingsSection["SenderAddress"]!;
            }
        }

        public bool Configured { get; private set; }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage) 
        {
            if (Configured)
            {
                var emailClient = new EmailClient(_connectionString);

                EmailSendOperation emailSendOperation = await emailClient.SendAsync(
                    WaitUntil.Completed,
                    senderAddress: _senderEmailAddress,
                    recipientAddress: email,
                    subject: subject,
                    htmlContent: htmlMessage);
            }
        }
    }
}
