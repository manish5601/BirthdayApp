using System.Net.Mail;
using System.Net;
using BirthdayApp.Models;

namespace BirthdayApp.Services
{
    public interface IEmailService
    {
        Task<(bool Success, string ErrorMessage)> SendBirthdayWishEmailAsync(EmailNotificationModel emailModel);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<(bool Success, string ErrorMessage)> SendBirthdayWishEmailAsync(EmailNotificationModel emailModel)
        {
            try
            {
                // Email configuration from appsettings.json
                var smtpServer = _configuration["Email:SmtpServer"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["Email:Username"];
                var smtpPassword = _configuration["Email:Password"];
                var fromEmail = _configuration["Email:FromEmail"] ?? smtpUsername;

                if (string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
                {
                    return (false, "SMTP username or password is missing in configuration.");
                }

                using var client = new SmtpClient(smtpServer, smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, "Birthday App"),
                    Subject = $"🎉 Happy Birthday, {emailModel.ToName}!",
                    IsBodyHtml = true,
                    Body = GenerateBirthdayEmailHtml(emailModel)
                };

                mailMessage.To.Add(emailModel.ToEmail);

                await client.SendMailAsync(mailMessage);
                return (true, "");
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Email sending failed: {ex}");
                return (false, ex.Message);
            }
        }

        private string GenerateBirthdayEmailHtml(EmailNotificationModel emailModel)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px; }}
        .wish-box {{ background: white; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #667eea; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
        .cake-icon {{ font-size: 48px; margin-bottom: 10px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='cake-icon'>🎂</div>
            <h1>Happy Birthday, {emailModel.ToName}!</h1>
            <p>Wishing you a fantastic day filled with joy and laughter!</p>
        </div>
        
        <div class='content'>
            <h2>You have a birthday wish from {emailModel.FromName}!</h2>
            
            <div class='wish-box'>
                <p><strong>Message:</strong></p>
                <p style='font-style: italic; color: #555;'>{emailModel.WishMessage}</p>
            </div>
            
            <p>Your birthday on {emailModel.BirthdayDate.ToString("MMMM dd, yyyy")} is being celebrated by your friends and family!</p>
            
            <p>Best wishes,<br>
            <strong>The Birthday App Team</strong></p>
        </div>
        
        <div class='footer'>
            <p>This email was sent from Birthday App - Never miss a birthday celebration!</p>
        </div>
    </div>
</body>
</html>";
        }
    }
} 