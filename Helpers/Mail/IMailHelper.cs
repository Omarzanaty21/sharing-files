using System;

namespace FileSharing.Helpers.Mail
{
    public interface IMailHelper
    {
        void SendMail(InputEmailMessage model);
    }
}