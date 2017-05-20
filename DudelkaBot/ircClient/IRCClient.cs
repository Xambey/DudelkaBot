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
using DudelkaBot.Messages;
using DudelkaBot.Logging;
using DudelkaBot.WebClients;

namespace DudelkaBot.ircClient
{
    public class IrcClient
    {
        #region Fields
        private string ipHost, userName, password;
        private int port;
        private int messageCount;
        private readonly int messageLimit = 10;
        #endregion

        #region References
        private TcpClient tcpClient;
        private StreamWriter outputStream;
        private StreamReader inputStream;
        private Queue<string> queueMessages = new Queue<string>();
        private Task process;
        private CancellationTokenSource tokenProcess;
        private CancellationTokenSource tokenRead;
        private static Dictionary<Users, Timer> whisperBlock = new Dictionary<Users, Timer>(); 
        #endregion

        #region Properties
        public TcpClient TcpClient { get => tcpClient; set => tcpClient = value; }
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
        public CancellationTokenSource TokenProcess { get => tokenProcess; protected set => tokenProcess = value; }
        public static Dictionary<Users, Timer> WhisperBlock { get => whisperBlock; protected set => whisperBlock = value; }
        public CancellationTokenSource TokenRead { get => tokenRead; set => tokenRead = value; }
        #endregion

        public void Reconnect()
        {
            StopProcess();
            TokenRead.Cancel();
            //tcpClient?.Client.Shutdown(SocketShutdown.Both);
            tcpClient.Dispose();
            InputStream.Dispose();
            OutputStream.Dispose();
            TokenRead = new CancellationTokenSource();

            GC.Collect(3,GCCollectionMode.Forced,true);
            Thread.Sleep(3000);
            if (isConnect())
            {
                //inputStream = new StreamReader(tcpClient.GetStream());
                //outputStream = new StreamWriter(tcpClient.GetStream());
                Thread.Sleep(2000);
                StartProcess();
                Thread.Sleep(2000);
                SignIn();
            }
        }

        public void StartProcess()
        {
            if (tokenProcess != null)
                tokenProcess.Dispose();
            tokenProcess = new CancellationTokenSource();
            process = new Task(Channel.Process, tokenProcess.Token);
            process.Start();
            Logger.ShowLineCommonMessage("Запущен обработчик сообщений...");
        }

        public void StopProcess()
        {
            if (process != null && !process.IsCompleted)
            {
                tokenProcess.Cancel();
                tokenProcess.Dispose();
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

        public bool isDisconnect()
        {
            try
            {
                if (tcpClient != null && tcpClient.Client != null && tcpClient.Connected)
                {
                    if (tcpClient.Client.Poll(0, SelectMode.SelectRead))
                    {
                        byte[] buff = new byte[1];
                        if (tcpClient.Client.Receive(buff, SocketFlags.Peek) == 0)
                        {
                            return true;
                        }
                        else
                            return false;
                    }
                    return false;
                }
                else
                    return true;
            }
            catch
            {
                return true;
            }
        }

        public bool isConnect()
        {
            if (tcpClient != null)
            {
                while (!tcpClient.Connected && isDisconnect())
                {
                    try
                    {
                        tcpClient.ConnectAsync(ipHost, port).Wait();
                        if (inputStream != null)
                            inputStream.Dispose();
                        if (outputStream != null)
                            outputStream.Dispose();
                        inputStream = new StreamReader(tcpClient.GetStream());
                        outputStream = new StreamWriter(tcpClient.GetStream());
                        Logger.ShowLineCommonMessage($"Соединение с сервером установлено...");
                    }
                    catch(ObjectDisposedException ex)
                    {
                        tcpClient = new TcpClient();
                        tcpClient.LingerState = new LingerOption(false, 0);
                        var color = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Logger.ShowLineCommonMessage("Подключение не удалось \n" + ex.Message);
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                    catch(Exception ex)
                    {
                        //if (ex.InnerException != null && ex.InnerException is SocketException)
                        //    return true;
                        var color = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Logger.ShowLineCommonMessage("Подключение не удалось \n" + ex.Message);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        if (ex.InnerException != null && (ex.InnerException as SocketException)?.SocketErrorCode == SocketError.IsConnected)
                        {
                            if (inputStream != null)
                                inputStream.Dispose();
                            if (outputStream != null)
                                outputStream.Dispose();
                            inputStream = new StreamReader(tcpClient.GetStream());
                            outputStream = new StreamWriter(tcpClient.GetStream());
                            return true;
                        }
                        Thread.Sleep(10000);
                    }
                }
            }
            else
            {
                tcpClient = new TcpClient();
                tcpClient.LingerState = new LingerOption(false, 0);
                while (!tcpClient.Connected && isDisconnect())
                {
                    try
                    {
                        tcpClient.ConnectAsync(ipHost, port).Wait();
                        if (inputStream != null)
                            inputStream.Dispose();
                        if (outputStream != null)
                            outputStream.Dispose();
                        inputStream = new StreamReader(tcpClient.GetStream());
                        outputStream = new StreamWriter(tcpClient.GetStream());
                        Console.WriteLine($"Соединение с сервером установлено...");
                    }
                    catch (Exception ex)
                    {
                        var color = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Подключение не удалось \n" + ex.Message);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        if (ex.InnerException != null && (ex.InnerException as SocketException)?.SocketErrorCode == SocketError.IsConnected)
                        {
                            if (inputStream != null)
                                inputStream.Dispose();
                            if (outputStream != null)
                                outputStream.Dispose();
                            inputStream = new StreamReader(tcpClient.GetStream());
                            outputStream = new StreamWriter(tcpClient.GetStream());
                            return true;
                        }
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

            SignIn();
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

        private void SendIrcMessage(string message, Message msg = null)
        { 
            if (isConnect())
            {
                if(messageCount++ < messageLimit) { 
                    outputStream.WriteLine(message);
                    outputStream.Flush();
                    Timer timer = new Timer(TimerTick, null, 0, 30000);
                }
                else if(msg != null)
                {
                    var u = Channel.IdReg.Match(msg.Data);
                    if (u.Success)
                        Channel.SendWhisperMessage(u.Groups["id"].Value, msg.UserName,message);
                }
                if (!message.StartsWith("PONG"))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    if (Channel.ActiveLog)
                        Logger.ShowLineCommonMessage(message);
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
            }
        }

        private void SendIrcMessage(string message, bool undetected, Message msg = null)
        {
            if (isConnect())
            {
                outputStream.WriteLine(message);
                outputStream.Flush();
                Timer timer = new Timer(TimerTick, null, 0, 30000);

                Console.ForegroundColor = ConsoleColor.Yellow;
                if(Channel.ActiveLog)
                    Logger.ShowLineCommonMessage(message);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public void SendChatMessage(string message, Message requestMsg) 
        {
            if(Channel.Channels[requestMsg.Channel].StatusBotOnChannel != enums.StatusBot.Sleep)
                SendIrcMessage(":" + userName + "!" + userName + "@" + userName + ".twi.twitch.tv PRIVMSG #" + requestMsg.Channel + " :@" + requestMsg.UserName + " " + message, requestMsg);
        }

        public void SendChatMessage(string message, string getter, Message requestMsg)
        {
            if (Channel.Channels[requestMsg.Channel].StatusBotOnChannel != enums.StatusBot.Sleep)
                SendIrcMessage(":" + userName + "!" + userName + "@" + userName + ".twi.twitch.tv PRIVMSG #" + requestMsg.Channel + " :@" + getter + " " + message, requestMsg);
        }

        public void SendChatBroadcastMessage(string message, Message requestMsg)
        {
            if (Channel.Channels[requestMsg.Channel].StatusBotOnChannel != enums.StatusBot.Sleep)
                SendIrcMessage(":" + userName + "!" + userName + "@" + userName + ".twi.twitch.tv PRIVMSG #" + requestMsg.Channel + " :" + message, requestMsg);
        }

        public void SendChatBroadcastMessage(string message, string channel)
        {
            if (Channel.Channels[channel].StatusBotOnChannel != enums.StatusBot.Sleep)
                SendIrcMessage(":" + userName + "!" + userName + "@" + userName + ".twi.twitch.tv PRIVMSG #" + channel + " :" + message);
        }

        public void SendChatBroadcastChatMessage(List<string> commands, Message requestMsg)
        {
            if (Channel.Channels[requestMsg.Channel].StatusBotOnChannel == enums.StatusBot.Sleep)
                return;
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
                    SendIrcMessage(buf,undetected:true);
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

        public string ReadMessageAsync()
        {
            try
            {
                if (TokenRead != null && TokenRead.IsCancellationRequested)
                    return null;
                var task = inputStream.ReadLineAsync();
                TokenRead = new CancellationTokenSource();
                task.Wait(60000, TokenRead.Token);

                return task.Result;
            }
            catch (ObjectDisposedException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Logger.ShowLineCommonMessage(ex.Message + ex.StackTrace + ex.Data);
                if (ex.InnerException != null)
                    Logger.ShowLineCommonMessage(ex.InnerException.Message + ex.InnerException.StackTrace + ex.InnerException.Data);
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Logger.ShowLineCommonMessage(ex.Message + ex.StackTrace + ex.Data);
                if (ex.InnerException != null)
                    Logger.ShowLineCommonMessage(ex.InnerException.Message + ex.InnerException.StackTrace + ex.InnerException.Data);
                return string.Empty;
            }
        }

        public static void BlockWhisperCancel(object obj)
        {
            WhisperBlock[obj as Users].Dispose();
            WhisperBlock.Remove(obj as Users);
        }
    }
}
