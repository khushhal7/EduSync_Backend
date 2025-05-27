// In Services/SendGridEmailService.cs (Backend Project)
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;
using EduSync.Settings; // Your namespace for EmailSettings
using Microsoft.Extensions.Configuration; // Required for IConfiguration

namespace EduSync.Services // Or your project's namespace
{
    public class SendGridEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly EmailSettings _emailSettings;

        public SendGridEmailService(IConfiguration configuration, IOptions<EmailSettings> emailSettings)
        {
            _configuration = configuration;
            _emailSettings = emailSettings.Value; // Correctly get the EmailSettings instance
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink)
        {
            // Retrieve the API key from User Secrets (via IConfiguration)
            // Ensure your secrets.json has: "SendGrid": { "ApiKey": "YOUR_KEY" }
            var apiKey = _configuration["SendGrid:ApiKey"];

            if (string.IsNullOrEmpty(apiKey))
            {
                // Log this error appropriately in a real application
                System.Diagnostics.Debug.WriteLine("SendGrid API Key not configured in User Secrets.");
                // For development, you might want to throw to make it obvious.
                // For production, handle more gracefully or ensure it's always set.
                throw new InvalidOperationException("SendGrid API Key is not configured. Please check your User Secrets file.");
            }
            if (string.IsNullOrEmpty(_emailSettings.SenderEmail) || string.IsNullOrEmpty(_emailSettings.SenderName))
            {
                System.Diagnostics.Debug.WriteLine("SenderEmail or SenderName not configured in EmailSettings.");
                throw new InvalidOperationException("SenderEmail or SenderName is not configured. Please check your configuration (e.g., User Secrets for EmailSettings).");
            }


            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName);
            var subject = "EduSync - Password Reset Request";
            var to = new EmailAddress(toEmail, userName); // Use the provided userName for the recipient name

            var plainTextContent = $"Hi {userName},\n\nPlease reset your password by clicking the following link or pasting it into your browser: {resetLink}\n\nIf you did not request a password reset, please ignore this email.\n\nThanks,\nThe EduSync Team";
            var htmlContent = $"<div style='font-family: Arial, sans-serif; line-height: 1.6;'>" +
                              $"<h2 style='color: #007bff;'>EduSync Password Reset</h2>" +
                              $"<p>Hi {userName},</p>" +
                              $"<p>We received a request to reset your password for your EduSync account. Please click the button below to choose a new password:</p>" +
                              $"<p style='text-align: center; margin: 20px 0;'>" +
                              $"<a href='{resetLink}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; font-weight: bold;'>Reset Password</a>" +
                              $"</p>" +
                              $"<p>If the button above doesn't work, you can copy and paste the following link into your browser's address bar:</p>" +
                              $"<p><a href='{resetLink}'>{resetLink}</a></p>" +
                              $"<p>This password reset link will expire in 1 hour (or as configured).</p>" +
                              $"<p>If you did not request a password reset, no further action is required; please ignore this email and your password will remain unchanged.</p>" +
                              $"<p>Thanks,<br/>The EduSync Team</p>" +
                              $"</div>";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            var response = await client.SendEmailAsync(msg);

            if (response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"Password reset email sent to {toEmail}. Status Code: {response.StatusCode}");
            }
            else
            {
                // Log detailed error information from SendGrid
                var responseBody = await response.Body.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Failed to send password reset email to {toEmail}. Status Code: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"SendGrid Response Headers: {response.Headers}");
                System.Diagnostics.Debug.WriteLine($"SendGrid Response Body: {responseBody}");

                // In a real application, you'd use a proper logging framework and might throw a more specific custom exception.
                throw new Exception($"Failed to send email via SendGrid: {response.StatusCode} - {responseBody}");
            }
        }
    }
}
