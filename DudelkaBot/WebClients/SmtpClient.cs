using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;
using System.Net;
using DudelkaBot.Logging;

namespace DudelkaBot.WebClients
{
    public static class SmtpClient
    {
        static SmtpClient() { }

        public static async void SendEmailAsync(string email, string subject, string message, string attached_file = null)
        {
            try
            {
                var emailMessage = new MimeMessage();

                emailMessage.From.Add(new MailboxAddress("DudelkaBot", "dmtgenerator@gmail.com"));
                emailMessage.To.Add(new MailboxAddress("", email));
                emailMessage.Subject = subject;
                var builder = new BodyBuilder() { TextBody = message };

                if (!string.IsNullOrEmpty(attached_file))
                    builder.Attachments.Add(attached_file);
                emailMessage.Body = builder.ToMessageBody();

                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    var credentials = new NetworkCredential(("dmtgenerator@gmail.com").Split('@')[0], "Passw0rdPassw0rd");
                    //client.LocalDomain = "some.domain.com";
                    await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.Auto).ConfigureAwait(false);
                    await client.AuthenticateAsync(credentials);
                    //client.Authenticate("dmtgenerator", "Passw0rdPassw0rd");
                    await client.SendAsync(emailMessage).ConfigureAwait(false);
                    await client.DisconnectAsync(true).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Logger.ShowLineCommonMessage(ex.StackTrace + ex.Data + ex.Message);
                if (ex.InnerException != null)
                    Logger.ShowLineCommonMessage(ex.InnerException.StackTrace + ex.InnerException.Data + ex.InnerException.Message);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
    }
}
