using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DFM.Shared.Configurations;

namespace DFM.Shared.Helper
{
    public class EmailProperty
    {
        public string? From { get; set; }
        public List<string>? To { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
    }
    public interface IEmailHelper
    {
        Task Send(EmailProperty emailProperty);
    }
    public class EmailHelper : IEmailHelper
    {
        private readonly SMTPConfig smtpConf;

        public EmailHelper(SMTPConfig smtpConf)
        {
            this.smtpConf = smtpConf;
        }
        public async Task Send(EmailProperty emailProperty)
        {
            SmtpClient smtp = new SmtpClient(smtpConf.Server, smtpConf.Port);
            smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential(smtpConf.Username, smtpConf.Password);

           
            MailMessage message = new MailMessage();
            message.From = new MailAddress(emailProperty.From!);
            message.Subject = emailProperty.Subject;
            foreach (var item in emailProperty.To!)
            {
                message.To.Add(item);
            }

            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(
              $"{emailProperty.Body} <br>",
              null,
              "text/html"
            );


            message.AlternateViews.Add(htmlView);

            try
            {
                await smtp.SendMailAsync(message);
            }
            catch (SmtpException)
            {
                throw;
            }
        }
    }
}
