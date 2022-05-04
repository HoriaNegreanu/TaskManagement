using TaskManagement.Models.Mail;

namespace TaskManagement.Services
{
    public interface IMailService
    {
        Task SendEmailAsync(MailRequest mailRequest);
    }
}
