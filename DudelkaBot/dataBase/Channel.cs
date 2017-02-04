using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DudelkaBot.ircClient;
using DudelkaBot.system;

namespace DudelkaBot.dataBase
{
    public class Channel
    {
        public static List<string> commands = new List<string>()
        {
            "!hi-приветствие",
            "!date-дата и время сервера",
            "!commands-список команд"
        };

        public static IrcClient ircClient;

        public string Name;
        public string data;
        public Thread Thread;
        public Status Status = Status.Unknown;

        private List<User> users = new List<User>();
        private List<User> moderators = new List<User>();
        private string iphost;
        private string userName;
        private string password;

        private int port;
        private Message lastMessage;

        private static List<Message> queueMessages = new List<Message>();
        private static List<Message> errorListMessages = new List<Message>();
        private static Channel viewChannel;
        public static Dictionary<string, Channel> channels = new Dictionary<string, Channel>();


        public Channel(string channelName, string iphost, int port, string userName, string password)
        {
            channels.Add(channelName, this);
            Name = channelName;
            this.iphost = iphost;
            this.port = port;
            this.password = password;
            this.userName = userName;
        }

        public void startShow()
        {
            viewChannel = this;
        }

        public void stopShow()
        {
            viewChannel = null;
        }

        public void Join()
        {
            if (ircClient == null)
            {
                ircClient = new IrcClient(iphost, port, userName, password);
                Task.Run(() => Process());
            }
            ircClient.joinRoom(Name);
        }

        private static void switchMessage(string data)
        {
            Message currentMessage = new Message(data);

            if (currentMessage.Channel == null || viewChannel.Name == currentMessage.Channel || !currentMessage.Success)
            {
                Console.WriteLine(data);
            }

            if (!currentMessage.Success)
            {
                errorListMessages.Add(currentMessage);
                return;
            }
            else if (currentMessage.UserName == "moobot" || currentMessage.UserName == "nightbot")
                return;

            if (currentMessage.Channel != null)
            {
                if(channels.ContainsKey(currentMessage.Channel))
                    channels[currentMessage.Channel].handler(currentMessage);
            }
            else
            {
                channels.First().Value?.handler(currentMessage);
            }

        } 
        private void handler(Message msg)
        { 
            switch (msg.Type)
            {
                case TypeMessage.JOIN:
                    lock (users)
                    {
                        if (!users.Exists(a => a.UserName == msg.UserName))
                        {
                            User user = new User(msg.UserName);
                            users.Add(user);
                        }
                    }
                    break;
                case TypeMessage.PART:
                    lock (users) {
                        if (users.Exists(a => a.UserName == msg.UserName))
                        {
                            users.Remove(users.Find(a => a.UserName == msg.UserName));
                        }
                    }
                    break;
                case TypeMessage.MODE:
                    lock (moderators)
                    {
                        if (!moderators.Exists(a => a.UserName == msg.UserName))
                        {
                            if (msg.Sign == "+")
                            {
                                User user = new User(msg.UserName);
                                moderators.Add(user);
                            }
                        }
                        else if (msg.Sign == "-")
                        {
                            moderators.RemoveAll(a => a.UserName == msg.UserName);
                        }
                    }
                    break;
                case TypeMessage.NAMES:
                    lock (users)
                    {
                        if (!users.Exists(a => a.UserName == msg.User1))
                        {
                            User user = new User(msg.User1);
                            users.Add(user);
                        }
                        if (!users.Exists(a => a.UserName == msg.User2))
                        {
                            User user = new User(msg.User2);
                            users.Add(user);
                        }
                        if (!users.Exists(a => a.UserName == msg.User3))
                        {
                            User user = new User(msg.User3);
                            users.Add(user);
                        }
                    }
                    break;
                case TypeMessage.NOTICE:
                    break;
                case TypeMessage.HOSTTARGET:
                    break;
                case TypeMessage.CLEARCHAT:
                    break;
                case TypeMessage.USERSTATE:
                    break;
                case TypeMessage.RECONNECT:
                    break;
                case TypeMessage.ROOMSTATE:
                    break;
                case TypeMessage.USERNOTICE:
                    lock (users)
                    {
                        if(users.Exists(a => a.UserName == msg.UserName))
                        {
                            users.Find(a => a.UserName == msg.UserName).Subscription = msg.Subscription;
                        }
                    }
                    break;
                case TypeMessage.Tags:
                    break;
                case TypeMessage.PRIVMSG:
                    lock (users)
                    {
                        if (!users.Exists(a => a.UserName == msg.UserName))
                        {
                            User user = new User(msg.UserName);
                            user.CountMessage++;
                            users.Add(user);
                        }
                        else
                        {
                            users.Find(a => a.UserName == msg.UserName).CountMessage++;
                        }

                        switch (msg.command)
                        {
                            case Command.help:
                                ircClient.sendChatBroadcastChatMessage(commands, msg);
                                break;
                            case Command.date:
                                ircClient.sendChatMessage(DateTime.Now.ToString(), msg);
                                break;
                            case Command.time:
                                break;
                            case Command.mystat:
                                break;
                            case Command.toplist:
                                lock (users)
                                {
                                    if (users.Count < 5)
                                        break;

                                    users = users.OrderByDescending(a => a.CountMessage).ToList();
                                }
                                List<string> toplist = new List<string>()
                            {
                                "Топ 5 самых общительных: " + users[0].UserName + "=" + users[0].CountMessage.ToString() + " ,",
                                users[1].UserName + "=" + users[1].CountMessage.ToString() + " ,",
                                users[2].UserName + "=" + users[2].CountMessage.ToString() + " ,",
                                users[3].UserName + "=" + users[3].CountMessage.ToString() + " ,",
                                users[4].UserName + "=" + users[4].CountMessage.ToString(),
                            };
                                ircClient.sendChatBroadcastChatMessage(toplist, msg);

                                break;
                            case Command.sexylevel:
                                break;
                            case Command.members:
                                ircClient.sendChatBroadcastMessage("Сейчас в чате " + users.Count.ToString() + " сексуалов и не только  blackufaPRIDE ", msg);
                                break;
                            case Command.unknown:
                                //errorListMessages.Add(lastMessage);
                                break;
                            default:
                                lock (errorListMessages)
                                    errorListMessages.Add(msg);
                                break;
                        }
                    }
                    break;
                case TypeMessage.GLOBALUSERSTATE:
                    break;
                case TypeMessage.UNKNOWN:
                    break;
                case TypeMessage.PING:
                    ircClient.pingResponse();
                    break;
                default:
                    break;
            }
        }

        private static void Process()
        {
            while (true)
            {
                try
                {

                    string s = ircClient.readMessage();


                    Task.Run(() => switchMessage(s));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.ReadLine();
                    return;
                }
            }
        }
    }
}
