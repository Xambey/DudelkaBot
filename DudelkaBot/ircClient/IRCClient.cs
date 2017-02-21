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
        private TcpClient tcpClient;
        private StreamWriter outputStream;
        private StreamReader inputStream;
        private int port;
        private string ipHost, userName, password;
        private Queue<string> queueMessages = new Queue<string>();

        private int messageCount;
        private const int messageLimit = 10;
        private Task process;
        private CancellationTokenSource token;

        public void reconnect(Action func)
        {
            stopProcess();
            inputStream.Dispose();
            outputStream.Dispose();
            tcpClient.Dispose();
            tcpClient = new TcpClient();
            
            if (isConnect())
            {
                inputStream = new StreamReader(tcpClient.GetStream());
                outputStream = new StreamWriter(tcpClient.GetStream());

                signIn();
            }
            startProcess(func);
        }

        public void startProcess(Action func)
        {
            token = new CancellationTokenSource();
            process = new Task(func, token.Token);
            process.Start();
        }

        public void stopProcess()
        {
            if (process != null && !process.IsCompleted)
            {
                token.Cancel();
            }
        }

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

        public bool isConnect()
        {
            if (tcpClient != null)
            {
                while (!tcpClient.Connected)
                {
                    try
                    {
                        tcpClient.ConnectAsync(ipHost, port).Wait();
                    }
                    catch(Exception ex)
                    {
                        var color = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Подключение не удалось \n" + ex.Message);
                        Console.ResetColor();
                    }
                }
                return true;
            }
            else
                return false;
        }

        public IrcClient(string ipHost, int port, string userName, string password)
        {
            Timer timer = new Timer(timerTick, null, 0, 30000);
            this.ipHost = ipHost;
            this.port = port;
            this.userName = userName;
            this.password = password;

            Console.OutputEncoding = Encoding.Unicode;

            tcpClient = new TcpClient();

            if (isConnect())
            {
                inputStream = new StreamReader(tcpClient.GetStream());
                outputStream = new StreamWriter(tcpClient.GetStream());

                signIn();
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
                lock (tcpClient)
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
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
        }

        public void joinRoom(string channel)
        {
            if (isConnect())
            {
                outputStream.WriteLine("JOIN #" + channel);
                outputStream.Flush();
                //sendChatBroadcastMessage("/me Свеженький Дуделка Бот входит в чат *Обнимашки* KappaPride ", channel);
            }
        }


        public void leaveRoom(string channel)
        {
            if(isConnect())
            {
                outputStream.WriteLine("PART #" + channel);
                outputStream.Flush();
            }
        }

        private void sendIrcMessage(string message)
        { 
            if (messageCount++ < messageLimit && isConnect())
            {
                outputStream.WriteLine(message);
                outputStream.Flush();
                Timer timer = new Timer(timerTick, null, 0, 30000);

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(message);
                Console.ResetColor();
            }
        }

        public void sendChatMessage(string message, Message requestMsg) 
        {
            sendIrcMessage(":" + userName + "!" + userName + "@" + userName + ".twi.twitch.tv PRIVMSG #" + requestMsg.Channel + " :@" + requestMsg.UserName + " " + message + "\r\n");
        }

        public void sendChatMessage(string message, string getter, Message requestMsg)
        {
            sendIrcMessage(":" + userName + "!" + userName + "@" + userName + ".twi.twitch.tv PRIVMSG #" + requestMsg.Channel + " :@" + getter + " " + message + "\r\n");
        }

        public void sendChatBroadcastMessage(string message, Message requestMsg)
        {
            sendIrcMessage(":" + userName + "!" + userName + "@" + userName + ".twi.twitch.tv PRIVMSG #" + requestMsg.Channel + " :" + message + "\r\n");
        }

        public void sendChatBroadcastMessage(string message, string channel)
        {
            sendIrcMessage(":" + userName + "!" + userName + "@" + userName + ".twi.twitch.tv PRIVMSG #" + channel + " :" + message + "\r\n");
        }

        public void sendChatBroadcastChatMessage(List<string> commands, Message requestMsg)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(":" + userName + "!" + userName + "@" + userName + ".twi.twitch.tv PRIVMSG #" + requestMsg.Channel + " :");

            foreach (var item in commands)
            {
                builder.Append(item + " ");
            }

            sendIrcMessage(builder.ToString());
        }

        //whispers
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void sendChatWhisperMessage(string message, Message requestMsg)
        {
            sendIrcMessage(":" + userName + "!" + userName + "@" + userName + ".twi.twitch.tv PRIVMSG #jtv" + " :/w " + requestMsg.UserName + " " + message + "\r\n");
        }

        public void sendChatWhisperMessage(string message, string username, string channel)
        {
            sendIrcMessage(":" + userName + "!" + userName + "@" + userName + ".twi.twitch.tv PRIVMSG #jtv" + " :/w" + username + " " + message + "\r\n");
        }

        public void sendChatWhisperMessage(List<string> commands, Message requestMsg)
        {
            string send = ":" + userName + "!" + userName + "@" + userName + ".twi.twitch.tv PRIVMSG #jtv" + " :/w " + requestMsg.UserName + " ";

            foreach (var item in commands)
            {
                sendIrcMessage(send + item);
                Thread.Sleep(300);
            }
        }

        public void pingResponse()
        {
            sendIrcMessage("PONG twi.twitch.tv\r\n");
        }

        public string readMessage()
        {
            //Console.WriteLine(inputStreamWhisper.ReadLine());
            return inputStream.ReadLine();
        }
    }
}
