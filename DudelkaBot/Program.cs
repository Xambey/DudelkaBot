using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IRCClient;
using System.Net.Sockets;
using System.Threading;

namespace DudelkaBot
{ 

    class Program
    {
        #region Variables
        static string userName = "DudelkaBot";
        static string password = "oauth:x2ha2yusryy5dir8xlhkg90rqfpkld";
        static Thread chatThread;
        static IrcClient ircClient = new IrcClient("irc.chat.twitch.tv", 6667, userName, password);
        static NetworkStream serverStream = ircClient.tcpClient.GetStream();
        static string readData = "";
        #endregion


        static void Main(string[] args)
        {
            int buffsize = 0;

            byte[] buff = new byte[10025];
            buffsize = ircClient.tcpClient.ReceiveBufferSize;

            ircClient.joinRoom("xiixalex");
            chatThread = new Thread(new ThreadStart(Process));
            chatThread.Start();
        }

        private static void Process()
        {
            while (true)
            {
                try
                {
                    readData = ircClient.readMessage();
                    int pos = readData.LastIndexOf(':');
                    readData = new string(readData.Skip(pos + 1).ToArray());

                    switch (readData)
                    {
                        case "!hi":
                            ircClient.sendChatMessage("ДуделкаБот работает!");
                            break;
                        case "!date":
                            ircClient.sendChatMessage(DateTime.Now.ToString());
                            break;
                        case "!просыпайся тварь":
                            ircClient.sendChatMessage("Да, хозяин!");
                            break;
                        case "!умри":
                            ircClient.sendChatMessage("умываю руки");
                            ircClient.tcpClient.Close();
                            return;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return;
                }
            }
        }
    }
}
