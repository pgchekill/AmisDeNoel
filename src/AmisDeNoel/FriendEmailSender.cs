using System.Net;
using System.Net.Mail;

namespace AmisDeNoel
{
    public static class FriendEmailSender
    {
        private const string GiverPlaceholder = ":::Giver:::";
        private const string ReceiverPlaceholder = ":::Receiver:::";
        internal static void SendEmails(List<ChristmasMatch> matches, string htmlTemplatePath, string user, string pass)
        {
            var htmlTemplate = File.ReadAllText(htmlTemplatePath);

            foreach (var match in matches)
            {
                var htmlContent = 
                        htmlTemplate
                            .Replace(GiverPlaceholder, match.Giver.Name)
                            .Replace(ReceiverPlaceholder, match.Receiver.Name);

                try
                {
                    Console.WriteLine($"Sening invite to {match.Giver.Name} ...");
                    using (var smtpClient = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtpClient.UseDefaultCredentials = false;
                        smtpClient.Credentials = new NetworkCredential()
                        {
                            UserName = user,
                            Password = pass,
                        };
                        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtpClient.EnableSsl = true;
                        
                        var mail = new MailMessage(user, match.Giver.Email, "Oh oh oh", htmlContent);
                        mail.IsBodyHtml = true;
                        smtpClient.Send(mail);
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine("{0}: {1}", e.ToString(), e.Message);
                }
            }
        }
    }
}
