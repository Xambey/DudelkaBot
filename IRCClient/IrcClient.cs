using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using System.Net;

namespace IRCClient
{
    public class IrcClient
    {
        public TcpClient tcpClient;
        private StreamWriter outputStream;
        private StreamReader inputStream;
        private int port;
        private string ipHost, userName, password, channel; 

        public IrcClient(string ipHost, int port, string userName, string password)
        {
            try {
                this.ipHost = ipHost;
                this.port = port;
                this.userName = userName;
                this.password = password;

                tcpClient = new TcpClient(ipHost, port);
                inputStream = new StreamReader(tcpClient.GetStream());
                outputStream = new StreamWriter(tcpClient.GetStream());

                Console.OutputEncoding = Encoding.Default;

                signIn();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
        }

        private void signIn()
        {
            if (outputStream == null)
            {
                throw new Exception("Error of sign in\n");
            }

            outputStream.WriteLine("PASS " + password);
            outputStream.WriteLine("NICK " + userName);
            outputStream.WriteLine("USER" + userName);
            outputStream.WriteLine("CAP REQ :twitch.tv/membership");
            outputStream.WriteLine("CAP REQ :twitch.tv/commands");
            outputStream.Flush();
        }

        public void joinRoom(string channel)
        {
            try {
                this.channel = channel;
                outputStream.WriteLine("JOIN #" + channel);
                outputStream.Flush();
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Error, client is shutdown...\n" +  ex.Message);
                return;
            }
        }

        public void leaveRoom()
        {
            if(outputStream != null)
                outputStream.Close();
            if (inputStream != null)
                inputStream.Close();
        }

        private void sendIrcMessage(string message)
        {
            try {
                if (outputStream == null)
                    throw new Exception("Output stream is empty...");
                outputStream.WriteLine(message);
                outputStream.Flush();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
        }

        public void sendChatMessage(string message)
        {
            sendIrcMessage(":" + userName + "!" + userName + "@" + userName + "twi.twitch.tv PRIVMSG #" + channel + " :" + message);
        }

        public void pingResponse()
        {
            sendIrcMessage("PONG twi.twitch.tv\r\n");
        }

        public string readMessage()
        {
            return inputStream.ReadLine();
        }
    }
}
