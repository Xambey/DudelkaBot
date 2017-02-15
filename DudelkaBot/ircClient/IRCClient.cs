using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace DudelkaBot.ircClient
{
    public class IrcClient
    {
        public TcpClient tcpClient;
        private StreamWriter outputStream;
        private StreamReader inputStream;
        private int port;
        private string ipHost, userName, password;
        private Queue<string> queueMessages = new Queue<string>();

        private int messageCount;
        private const int messageLimit = 15;

        private void timerTick(object state)
        {

            messageCount = 0;
            if (isConnect())
            {
                lock (queueMessages)
                    while (messageCount < messageLimit && queueMessages.Count != 0)
                    {
                        lock (outputStream)
                        {
                            outputStream.WriteLine(queueMessages.Dequeue());
                            outputStream.Flush();
                        }
                        messageCount++;
                    }
            }
        }

        private bool isConnect()
        {
            if(tcpClient != null)
                while (!tcpClient.Connected)
                {
                    try
                    {
                        tcpClient.ConnectAsync(ipHost, port).Wait();
                    }
                    finally
                    {
                        var color = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Подключение не удалось");
                        Thread.Sleep(5000);
                    }
                }
            return true;
        }

        public IrcClient(string ipHost, int port, string userName, string password)
        {
            try
            {
                Timer timer = new Timer(timerTick, null, 0, 30000);
                this.ipHost = ipHost;
                this.port = port;
                this.userName = userName;
                this.password = password;

                Console.OutputEncoding = Encoding.Unicode;

                tcpClient = new TcpClient();

                tcpClient.ConnectAsync(ipHost, port).Wait();

                if (isConnect())
                {
                    inputStream = new StreamReader(tcpClient.GetStream());
                    outputStream = new StreamWriter(tcpClient.GetStream());

                    signIn();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void signIn()
        {
            if (isConnect())
            {
                outputStream.WriteLine("PASS " + password);
                outputStream.WriteLine("NICK " + userName);
                outputStream.WriteLine("USER " + userName);
                outputStream.WriteLine("CAP REQ :twitch.tv/membership");
                outputStream.WriteLine("CAP REQ :twitch.tv/commands");
                outputStream.WriteLine("CAP REQ :twitch.tv/tags");
                outputStream.Flush();
            }
        }

        public void updateMembers()
        {
            try
            {
                while (!tcpClient.Connected)
                {
                    try
                    {
                        tcpClient.ConnectAsync(ipHost, port).Wait();
                    }
                    finally
                    {
                        var color = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Подключение не удалось");
                    }
                }
                outputStream.WriteLine("CAP REQ :twitch.tv/membership");
                outputStream.Flush();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
        }

        public void joinRoom(string channel)
        {
            if (outputStream != null)
            {
                while (!tcpClient.Connected)
                {
                    try
                    {
                        tcpClient.ConnectAsync(ipHost, port).Wait();
                    }
                    finally
                    {
                        var color = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Подключение не удалось");
                    }
                }
                outputStream.WriteLine("JOIN #" + channel);
                outputStream.Flush();
            }
        }

        public void leaveRoom(string channel)
        {
            if(outputStream != null)
            {
                outputStream.WriteLine("PART #" + channel);
                outputStream.Flush();
            }
        }

        private void sendIrcMessage(string message)
        {
            while (!tcpClient.Connected)
            {
                try
                {
                    tcpClient.ConnectAsync(ipHost, port).Wait();
                }
                finally
                {
                    var color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Подключение не удалось");
                }
            }


            if (messageCount++ < messageLimit)
            {
                lock (outputStream)
                {
                    outputStream.WriteLine(message);
                    outputStream.Flush();
                }
                Timer timer = new Timer(timerTick, null, 0, 30000);
            }
            else
            {
                queueMessages.Enqueue(message);
            }
        }

        public void sendChatMessage(string message, Message requestMsg) 
        {
            sendIrcMessage(":" + userName + "!" + userName + "@" + userName + "twi.twitch.tv PRIVMSG #" + requestMsg.Channel + " :@" + requestMsg.UserName + " " + message + "\r\n");
        }

        public void sendChatMessage(string message, string getter, Message requestMsg)
        {
            sendIrcMessage(":" + userName + "!" + userName + "@" + userName + "twi.twitch.tv PRIVMSG #" + requestMsg.Channel + " :@" + getter + " " + message + "\r\n");
        }

        public void sendChatBroadcastMessage(string message, Message requestMsg)
        {
            sendIrcMessage(":" + userName + "!" + userName + "@" + userName + "twi.twitch.tv PRIVMSG #" + requestMsg.Channel + " :" + message + "\r\n");
        }

        public void sendChatBroadcastMessage(string message, string channel)
        {
            sendIrcMessage(":" + userName + "!" + userName + "@" + userName + "twi.twitch.tv PRIVMSG #" + channel + " :" + message + "\r\n");
        }

        public void sendChatBroadcastChatMessage(List<string> commands, Message requestMsg)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(":" + userName + "!" + userName + "@" + userName + "twi.twitch.tv PRIVMSG #" + requestMsg.Channel + " :");

            foreach (var item in commands)
            {
                builder.Append(item + " ");
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine(builder.ToString());

            sendIrcMessage(builder.ToString());
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
