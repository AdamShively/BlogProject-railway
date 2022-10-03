
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace BlogProject.Services
{   
    public class EmailService : IBlogEmailSender
    {

        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendContactEmailAsync(string emailFrom, string name, string subject, string htmlMessage)
        {
            var email = new MimeMessage();
            var mail = _config["Mail"];

            email.Sender = MailboxAddress.Parse(mail);
            email.To.Add(MailboxAddress.Parse(mail));
            email.Subject = subject;

            var builder = new BodyBuilder();
            builder.HtmlBody = $"<b>{name}</b> has sent you an email and can be reached at: <b>{emailFrom}</b><br/><br/>{htmlMessage}";

            email.Body = builder.ToMessageBody();

            var port = Int32.Parse(_config["MailPort"]);

            using var smtp = new SmtpClient();
            smtp.Connect(_config["MailHost"], port, SecureSocketOptions.StartTls);
            smtp.Authenticate(mail, _config["MailPassword"]);

            await smtp.SendAsync(email);

            smtp.Disconnect(true);
        }

        public async Task SendEmailAsync(string emailTo, string subject, string htmlMessage)
        {
            var email = new MimeMessage();
            var mail = _config["Mail"];

            email.Sender = MailboxAddress.Parse(mail);
            email.To.Add(MailboxAddress.Parse(emailTo));
            email.Subject = subject;

            var builder = new BodyBuilder() { HtmlBody = htmlMessage };

            email.Body = builder.ToMessageBody();

            var port = Int32.Parse(_config["MailPort"]);

            using var smtp = new SmtpClient();
            smtp.Connect(_config["MailHost"], port, SecureSocketOptions.StartTls);
            smtp.Authenticate(mail, _config["MailPassword"]);

            await smtp.SendAsync(email);
            smtp.Disconnect(true);
        }
    }
}
