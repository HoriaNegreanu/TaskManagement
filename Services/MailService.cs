using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using TaskManagement.Models;
using TaskManagement.Models.Mail;

namespace TaskManagement.Services
{
    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public MailService(IOptions<MailSettings> mailSettings, IHttpContextAccessor httpContextAccessor)
        {
            _mailSettings = mailSettings.Value;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task SendEmailAsync(MailRequest mailRequest)
        {
            var displayName = _mailSettings.DisplayName;
            var fromMail = _mailSettings.Mail;

            var email = new MimeMessage();
            //email.Sender = MailboxAddress.Parse(_mailSettings.Mail);
            email.From.Add(new MailboxAddress(displayName, fromMail));
            email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));
            email.Subject = mailRequest.Subject;
            var builder = new BodyBuilder();
            builder.HtmlBody = mailRequest.Body;
            email.Body = builder.ToMessageBody();
            using var smtp = new SmtpClient();
            smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);
            await smtp.SendAsync(email);
            smtp.Disconnect(true);
        }

        public async void SendEmailTaskAvailable(string destination, TaskItem taskItem)
        {
            string myHostUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}/TaskItems/Details/" + taskItem.ID;
            //create mail request
            var mailRequest = new MailRequest();
            var mailBody = "Task " + taskItem.EmailBody() + " is now available. Task link: " + myHostUrl;
            mailRequest.Body = mailBody;
            mailRequest.Subject = "Task Available";
            mailRequest.ToEmail = destination;

            //var mailService = new MailService(_mailSettings);
            SendEmailAsync(mailRequest);
        }

        public async void SendEmailTaskClosed(string destination, TaskItem taskItem)
        {
            string myHostUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}/TaskItemsClosed/Details/" + taskItem.ID;
            //create mail request
            var mailRequest = new MailRequest();
            var mailBody = "Task " + taskItem.EmailBody() + " has been closed. Task link: " + myHostUrl;
            mailRequest.Body = mailBody;
            mailRequest.Subject = "Task Available";
            mailRequest.ToEmail = destination;

           //var mailService = new MailService(_mailSettings);
            SendEmailAsync(mailRequest);
        }
    }
}
