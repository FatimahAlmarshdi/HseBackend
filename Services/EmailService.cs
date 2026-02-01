using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace HseBackend.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailWithAttachmentAsync(string toEmail, string subject, string body, string attachmentPath)
        {
            if (string.IsNullOrEmpty(toEmail)) return;

            try
            {
                // Retrieve SMTP settings from configuration
                // For demonstration, these can be set in appsettings.json
                var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUser = _configuration["Email:SmtpUser"]; // e.g. your_email@gmail.com
                var smtpPass = _configuration["Email:SmtpPass"]; // App Password

                if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
                {
                    // Log or handle missing credentials
                    Console.WriteLine($"[Email Service] Missing credentials. Simulating email to {toEmail}");
                    return;
                }

                using (var client = new SmtpClient(smtpHost, smtpPort))
                {
                    client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                    client.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(smtpUser, "HSE App System"),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(toEmail);

                    if (!string.IsNullOrEmpty(attachmentPath) && System.IO.File.Exists(attachmentPath))
                    {
                        mailMessage.Attachments.Add(new Attachment(attachmentPath));
                    }

                    await client.SendMailAsync(mailMessage);
                    Console.WriteLine($"[Email Service] Email sent to {toEmail} successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Email Service] Error sending email: {ex.Message}");
            }
        }
    }
}
