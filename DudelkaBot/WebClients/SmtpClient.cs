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

        public static async void SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                var emailMessage = new MimeMessage();

                emailMessage.From.Add(new MailboxAddress("DudelkaBot", "dmtgenerator@gmail.com"));
                emailMessage.To.Add(new MailboxAddress("", email));
                emailMessage.Subject = subject;
                emailMessage.Body = new TextPart("plain") { Text = message };


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
