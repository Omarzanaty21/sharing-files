using System;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace FileSharing.Helpers.Mail{
    public class MailHelper : IMailHelper
    {
        private readonly IConfiguration config;

        public MailHelper(IConfiguration config)
        {
            this.config = config;
        }
        public void SendMail(InputEmailMessage model)
        {
            using(SmtpClient client = new SmtpClient(config.GetValue<string>("Mail:Host"), config.GetValue<int>("Mail:Port")))
            {
                var msg = new MailMessage();
                msg.To.Add(model.Email);
                msg.Subject = model.Subject;
                msg.Body = model.Body;
                msg.IsBodyHtml = true;
                msg.From = new MailAddress(config.GetValue<string>("Mail:From"), config.GetValue<string>("Mail:Sender"), System.Text.Encoding.UTF8);
                client.Credentials = new System.Net.NetworkCredential(config.GetValue<string>("Mail:From"), config.GetValue<string>("Mail:PWD"));
                client.Send(msg);
            }
        }
    }
}