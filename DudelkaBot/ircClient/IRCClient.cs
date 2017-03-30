using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using DudelkaBot.dataBase.model;
using DudelkaBot.system;

namespace DudelkaBot.ircClient
{
    public class IrcClient
    {
        #region Fields
        private TcpClient tcpClient;
        private StreamWriter outputStream;
        private StreamReader inputStream;
        private string ipHost, userName, password;
        private Queue<string> queueMessages = new Queue<string>();
        private int port;
        private int messageCount;
        private readonly int messageLimit = 10;
        private Task process;
        private CancellationTokenSource token;

        private static Dictionary<Users, Timer> whisperBlock = new Dictionary<Users, Timer>();
        #endregion

        #region Properties
        public TcpClient TcpClient { get => tcpClient; protected set => tcpClient = value; }
        public StreamWriter OutputStream { get => outputStream; protected set => outputStream = value; }
        public StreamReader InputStream { get => inputStream; protected set => inputStream = value; }
        public string IpHost { get => ipHost; protected set => ipHost = value; }
        public string UserName { get => userName; protected set => userName = value; }
        public string Password { get => password; protected set => password = value; }
        public Queue<string> QueueMessages { get => queueMessages; protected set => queueMessages = value; }
        public int Port { get => port; protected set => port = value; }
        public int MessageCount { get => messageCount; protected set => messageCount = value; }
        public int MessageLimit => messageLimit;
        public Task Process { get => process; protected set => process = value; }
        public CancellationTokenSource Token { get => token; protected set => token = value; }
        public static Dictionary<Users, Timer> WhisperBlock { get => whisperBlock; protected set => whisperBlock = value; } 
        #endregion

        public void Reconnect(Action func)
        {
            StopProcess();
            inputStream.Dispose();
            outputStream.Dispose();
            tcpClient.Dispose();
            Logger.StopWrite();
            tcpClient = new TcpClient();
            
            if (isConnect())
            {
                inputStream.DiscardBufferedData();
                inputStream = new StreamReader(tcpClient.GetStream());
                outputStream = new StreamWriter(tcpClient.GetStream());

                SignIn();
            }
            StartProcess(func);
        }

        public void StartProcess(Action<Task> func)
        {
            token = new CancellationTokenSource();
            process = new Task(func, token.Token);
            process.Start();
            Logger.ShowLineCommonMessage("Запущен обработчик сообщений...");
        }

        public void StopProcess()
        {
            if (process != null && !process.IsCompleted)
            {
                token.Cancel();
                Logger.ShowLineCommonMessage("Обработчик сообщений остановлен...");
            }
        }

        private void TimerTick(object state)
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
                        tcpClient.Client.ConnectAsync(ipHost, port).Wait();
                        Logger.ShowLineCommonMessage($"Соединение с сервером установлено...");
                    }
                    catch(Exception ex)
                    {
                        var color = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Logger.ShowLineCommonMessage("Подключение не удалось \n" + ex.Message);
                        Console.ResetColor();
                        Thread.Sleep(10000);
                    }
                }
            }
            else
            {
                tcpClient = new TcpClient();
                while (!tcpClient.Connected)
                {
                    try
                    {
                        tcpClient.ConnectAsync(ipHost, port);
                        Console.WriteLine($"Соединение с сервером установлено...");
                    }
                    catch (Exception ex)
                    {
                        var color = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Подключение не удалось \n" + ex.Message);
                        Console.ResetColor();
                        Thread.Sleep(10000);
                    }
                }
            }
            
            return true; 
        }

        public IrcClient(string ipHost, int port, string userName, string password)
        {
            this.ipHost = ipHost;
            this.port = port;
            this.userName = userName;
            this.password = password;
            Timer timer = new Timer(TimerTick, null, 30000, 30000);

            tcpClient = new TcpClient();

            if (isConnect())
            {
                inputStream = new StreamReader(tcpClient.GetStream());
                outputStream = new StreamWriter(tcpClient.GetStream());
                SignIn();
            }
        }

        public void SignIn()
        {
            if (isConnect())
            {
                outputStream.WriteLine("PASS " + password);
                outputStream.WriteLine("NICK " + userName);
                outputStream.WriteLine("CAP REQ :twitch.tv/membership");
                outputStream.WriteLine("CAP REQ :twitch.tv/commands");
                outputStream.WriteLine("CAP REQ :twitch.tv/tags");
                //outputStream.WriteLine("");
                outputStream.Flush();
                Logger.ShowLineCommonMessage("Инициализация аккаунта...");
            }
        }

        public void JoinRoom(string channel)
        {
            if (isConnect())
            {
                outputStream.WriteLine("JOIN #" + channel);
                outputStream.Flush();
                //sendChatBroadcastMessage("/me Свеженький Дуделка Бот входит в чат *Обнимашки* KappaPride ", channel);
            }
        }

        public void LeaveRoom(string channel)
        {
            if (isConnect())
            {
                outputStream.WriteLine("PART #" + channel);
                outputStream.Flush();
            }
        }

        private void SendIrcMessage(string message)
        { 

            if (isConnect() && messageCount++ < messageLimit)
            {
                outputStream.WriteLine(message);
                outputStream.Flush();
                Timer timer = new Timer(TimerTick, null, 0, 30000);

                Console.ForegroundColor = ConsoleColor.Yellow;
                Logger.ShowLineCommonMessage(message);
                Console.ResetColor();
            }
        }

        private void SendIrcMessage(string message, bool undetected)
        {
            if (isConnect())
            {
                outputStream.WriteLine(message);
                outputStream.Flush();
                Timer timer = new Timer(TimerTick, null, 0, 30000);

                Console.ForegroundColor = ConsoleColor.Yellow;
                Logger.ShowLineCommonMessage(message);
                Console.ResetColor();
            }
        }

        public void SendChatMessage(string message, Message requestMsg) 
        {
            SendIrcMessage(":" + userName + "!" + userName + "@" + userName + ".twi.twitch.tv PRIVMSG #" + requestMsg.Channel + " :@" + requestMsg.UserName + " " + message);
        }

        public void SendChatMessage(string message, string getter, Message requestMsg)
        {
            SendIrcMessage(":" + userName + "!" + userName + "@" + userName + ".twi.twitch.tv PRIVMSG #" + requestMsg.Channel + " :@" + getter + " " + message);
        }

        public void SendChatBroadcastMessage(string message, Message requestMsg)
        {
            SendIrcMessage(":" + userName + "!" + userName + "@" + userName + ".twi.twitch.tv PRIVMSG #" + requestMsg.Channel + " :" + message);
        }

        public void SendChatBroadcastMessage(string message, string channel)
        {
            SendIrcMessage(":" + userName + "!" + userName + "@" + userName + ".twi.twitch.tv PRIVMSG #" + channel + " :" + message);
        }

        public void SendChatBroadcastChatMessage(List<string> commands, Message requestMsg)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(":" + userName + "!" + userName + "@" + userName + ".twi.twitch.tv PRIVMSG #" + requestMsg.Channel + " :");

            foreach (var item in commands)
            {
                builder.Append(item + " ");
            }

            SendIrcMessage(builder.ToString());
        }
        
        //whispers
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void SendChatWhisperMessage(string message, ulong id_user, Message requestMsg)
        {
            if (WhisperBlock.Any(a => a.Key.Username == requestMsg.UserName))
                return;
            SendIrcMessage(":" + userName + "!" + userName + "@" + userName + ".twi.twitch.tv PRIVMSG #jtv" + " :/w " + requestMsg.UserName + " " + message);
        }

        public void SendChatWhisperMessage(string message, string username, ulong id_user, string channel)
        {
            if (WhisperBlock.Any(a => a.Key.Username == username))
                return;
            SendIrcMessage(":" + userName + "!" + userName + "@" + userName + ".twi.twitch.tv PRIVMSG #jtv" + " :/w " + username + " " + message);
        }

        public void SendChatWhisperMessage(List<string> commands, ulong id_user, Message requestMsg)
        {
            if (WhisperBlock.Any(a => a.Key.Username == requestMsg.UserName))
                return;
            int i = 1;
            string send = /* "@badges=;color=;display-name=DudelkaBot;emotes=;mod=0;room-id=;subscriber=0;turbo=0;user-id=145466944;user-type=" + */":" + userName + "!" + userName + "@" + userName + ".twi.twitch.tv PRIVMSG #jtv" + " :/w " + requestMsg.UserName + " ";
            string buf = send;
            foreach (var item in commands)
            {
                buf += item + "; ";
                if (i % 3 == 0)
                {
                    SendIrcMessage(buf, true);
                    buf = send;
                    Thread.Sleep(500);
                }
                i++;
            }
            if(buf != send)
                SendIrcMessage(buf, true);
            var user = new Users(requestMsg.UserName);
            WhisperBlock.Add(user, new Timer(BlockWhisperCancel,user, 60000, 60000));
        }

        public void PingResponse()
        {
            SendIrcMessage("PONG twi.twitch.tv");
        }

        public string ReadMessage()
        {
            return inputStream.ReadLine();
        }

        public static void BlockWhisperCancel(object obj)
        {
            WhisperBlock[obj as Users].Dispose();
            WhisperBlock.Remove(obj as Users);
        }
    }
}
