using System.Net.Mail;

namespace CoreIdentity.Helpers
{
    public static class PasswordReset
    {
        public static void PasswordResetSendEmail(string link,string email)
        {
            MailMessage mail = new MailMessage();

            SmtpClient smtpClient = new SmtpClient("mail.fatihdeniz.com");

            mail.From = new MailAddress("blog@fatihdeniz.com");
            mail.To.Add(email);

            mail.Subject = $"www.fatihdeniz.com::Şifre sıfırlama";
            mail.Body="<h2>Şifrenizi yenilemek için lütfen aşağiıaki linke tıklayınız.</h2><hr/>";
            mail.Body += $"<a href='{link}'>şifre yenileme linki</a>";
            mail.IsBodyHtml = true;
            smtpClient.Port = 8889;
            smtpClient.Credentials = new System.Net.NetworkCredential("blog@fatihdeniz.com", "$Blog_2021!");

            smtpClient.Send(mail);
        }
    }
}


//Details : https://stackoverflow.com/questions/32260/sending-email-in-net-through-gmail

#region MyRegion
/* public static class PasswordReset
    {
        public static void PasswordResetSendEmail(string link,string email,string userName)
        {
            var fromAddress = new MailAddress("mail@gmail.com", "Your name");
            var toAddress = new MailAddress(email, userName);
            const string fromPassword = "yourpassword";
            const string subject = "www.domain.com::Reset Password";
            string body = $"<a href='{link}'>Reset Password Link</a>";
            
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
            {
                smtp.Send(message);
            }
        }
    }
*/


#endregion