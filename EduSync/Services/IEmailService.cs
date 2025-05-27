// In Services/IEmailService.cs (Backend Project)
using System.Threading.Tasks;

namespace EduSync.Services // Or your project's namespace
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink);
        // We can add other email sending methods here later if needed
        // e.g., Task SendWelcomeEmailAsync(string toEmail, string userName);
    }
}