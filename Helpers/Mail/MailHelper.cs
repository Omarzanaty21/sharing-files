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
                
                msg.From = new MailAddress(config["Mail:From"], config["Mail:Sender"], System.Text.Encoding.UTF8);
                client.UseDefaultCredentials = false;
                client.EnableSsl = true;
                client.Credentials = new System.Net.NetworkCredential(config["Mail:From"], config["Mail:PWD"]);
                client.Send(msg);
            }
        }
    }
}