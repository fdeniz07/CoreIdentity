﻿using System.Net.Mail;

namespace CoreIdentity.Helpers
{
    public static class EmailConfirmation
    {
        public static void SendEmail(string link, string email)
        {
            MailMessage mail = new MailMessage();

            SmtpClient smtpClient = new SmtpClient("mail.fatihdeniz.com");

            mail.From = new MailAddress("blog@fatihdeniz.com");
            mail.To.Add(email);

            mail.Subject = $"www.fatihdeniz.com::Email Doğrulama";
            mail.Body = "<h2>Email adresinizi doğrulamak için lütfen aşağıdaki linke tıklayınız.</h2><hr/>";
            mail.Body += $"<a href='{link}'>email Dogrulama linki</a>";
            mail.IsBodyHtml = true;
            smtpClient.Port = 8889;
            smtpClient.Credentials = new System.Net.NetworkCredential("blog@fatihdeniz.com", "$Blog_2021!");

            smtpClient.Send(mail);
        }
    }
}
