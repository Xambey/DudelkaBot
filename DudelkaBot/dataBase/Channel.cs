using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DudelkaBot.ircClient;
using DudelkaBot.system;
using DudelkaBot.resources;
using System.Text;

namespace DudelkaBot.dataBase
{
    public class Channel
    {
        private static Random rand = new Random();
        public static List<string> commands = new List<string>()
        {
            "!vote [Тема голосования]:[время в мин]:[variant1,variant2,variantn] (через , без пробелов) - голосование",
            "!sexylevel - ваш уровень сексуальности на канале",
            "!date - дата и время сервера",
            "!help - список команд",
            "!members - кол-во чатеров в зале",
            "!mystat - ваша статистика за все время",
            "!toplist - топ общительных за все время",
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
        public Dictionary<string, List<User>> voteResult = new Dictionary<string, List<User>>();
        public bool VoteActive = false;
        public Timer VoteTimer;


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

        private int sexyLevel(string username)
        {
            int level = 0;
            lock (users)
            {
                User user = users.Find(a => a.UserName == username);

                if (user.Subscription >= 12)
                    level += 100;
                else if (user.Subscription >= 10)
                    level += 80;
                else if (user.Subscription >= 8)
                    level += 70;
                else if (user.Subscription >= 6)
                    level += 60;
                else if (user.Subscription >= 4)
                    level += 40;
                else if (user.Subscription >= 3)
                    level += 30;
                else if (user.Subscription >= 2)
                    level += 20;
                else if (user.Subscription >= 1)
                    level += 10;

                if (user.CountMessage >= 3000)
                    level += 100;
                else if (user.CountMessage >= 2500)
                    level += 80;
                else if (user.CountMessage >= 2000)
                    level += 60;
                else if (user.CountMessage >= 1500)
                    level += 50;
                else if (user.CountMessage >= 1000)
                    level += 40;
                else if (user.CountMessage >= 800)
                    level += 30;
                else if (user.CountMessage >= 600)
                    level += 20;
                else if (user.CountMessage >= 300)
                    level += 10;
            }
            return level;
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
        
        private bool isUserVote(Message msg)
        {
            lock (voteResult)
            {
                foreach (var item in voteResult)
                {
                    if (item.Value.All(a => a.UserName != msg.UserName) == false)
                    {
                        return true;
                    }
                }
                return false;
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

                        if(msg.UserName == "twitchnotify")
                        {
                            User user = users.Find(a => a.UserName == msg.SubscriberName);
                            lock (user)
                            {
                                if (msg.Subscription != 0)
                                {
                                    user.Subscription = msg.Subscription;
                                }
                                else
                                    user.Subscription = 1;
                            }
                            break;
                        }
                        else if (VoteActive)
                        {
                            if((voteResult.ContainsKey(msg.Msg) || (msg.Msg.All(char.IsDigit) && int.Parse(msg.Msg) <= voteResult.Count ? true : false)) && isUserVote(msg) == false)
                            {
                                if (msg.Msg.All(char.IsDigit))
                                {
                                    voteResult[voteResult.ElementAt(int.Parse(msg.Msg) - 1).Key].Add(new User(msg.UserName));
                                }
                                else
                                    voteResult[msg.Msg].Add(new User(msg.UserName));
                            }
                        }
                        
                        switch (msg.command)
                        {
                            case Command.vote:
                                lock (moderators)
                                {
                                    if ((moderators.Find(a => a.UserName == msg.UserName) != null || msg.UserName == "dudelka_krasnaya" || msg.UserName == Name) && msg.VoteActive && !VoteActive)
                                    {
                                        VoteTimer = new Timer(stopVote, null, msg.Time * 60000, msg.Time * 60000);
                                        lock (voteResult)
                                        {
                                            for(int i = 0; i < msg.variants.Count; i++)
                                            {
                                                voteResult.Add(msg.variants[i], new List<User>());
                                                msg.variants[i] = (i+1).ToString() + ")" + msg.variants[i];
                                            }
                                            msg.variants.Insert(0, "/me Начинается голосование по теме: '" + msg.Theme + "' Время: " + msg.Time.ToString() + "мин." + " Варианты: ");
                                            msg.variants.Add(" Пишите НОМЕР варианта или САМ вариант!.");
                                        }
                                        ircClient.sendChatBroadcastChatMessage(msg.variants, msg);
                                        VoteActive = true;
                                    }
                                }
                                break;
                            case Command.help:
                                ircClient.sendChatBroadcastChatMessage(commands, msg);
                                break;
                            case Command.date:
                                ircClient.sendChatMessage(DateTime.Now.ToString(), msg);
                                break;
                            case Command.mystat:
                                lock (users) {
                                    User user = users.Find(a => a.UserName == msg.UserName);
                                    lock(user)
                                        ircClient.sendChatMessage("Вы написали " + user.CountMessage.ToString() + " сообщений на канале" + (user.Subscription > 0 ? ", также вы сексуальны уже " + user.Subscription.ToString() + "месяцев KappaPride" : ""), msg);
                                }
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
                                "Топ 5 самых общительных(сообщения): " + users[0].UserName + " = " + users[0].CountMessage.ToString() + " ,",
                                users[1].UserName + " = " + users[1].CountMessage.ToString() + " ,",
                                users[2].UserName + " = " + users[2].CountMessage.ToString() + " ,",
                                users[3].UserName + " = " + users[3].CountMessage.ToString() + " ,",
                                users[4].UserName + " = " + users[4].CountMessage.ToString(),
                            };
                                ircClient.sendChatBroadcastChatMessage(toplist, msg);

                                break;
                            case Command.sexylevel:
                                int level = sexyLevel(msg.UserName);
                                ircClient.sendChatMessage("Ваш уровень сексуальности " + level.ToString() + " из 200" + ", вы настолько сексуальны, что: " + getLevelMessage(level), msg);
                                
                                break;
                            case Command.members:
                                ircClient.sendChatBroadcastMessage("Сейчас в чате " + users.Count.ToString() + " сексуалов и не только KappaPride ", msg);
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

        private void stopVote(object s)
        {
            lock (voteResult)
            {
                VoteActive = false;
                StringBuilder builder = new StringBuilder(voteResult.Count);

                builder.Append("/me Голосование окончено! Результаты: ");
                string win = voteResult.First().Key;
                int max = voteResult.First().Value.Count;
                foreach (var item in voteResult)
                {
                    int current = item.Value.Count;
                    if (max < current)
                    {
                        max = current;
                        win = item.Key;
                    }
                    builder.Append(item.Key + " - " + current + ",");
                }
                builder.Append(" Победил - " + win + " с результатом в " + max.ToString() + " голосов.");

                ircClient.sendChatBroadcastMessage(builder.ToString(), Name);
                voteResult.Clear();
                VoteTimer.Dispose();
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

        private string getLevelMessage(int level)
        {
            string buf = "";
            
            switch (level)
            {
                case 200:
                    buf = Answers.level200[rand.Next(0, Answers.level200.Count - 1)];
                    break;
                case 190:
                    buf = Answers.level190[rand.Next(0, Answers.level190.Count - 1)]; 
                    break;
                case 180:
                    buf = Answers.level180[rand.Next(0, Answers.level180.Count - 1)];
                    break;
                case 170:
                    buf = Answers.level170[rand.Next(0, Answers.level170.Count - 1)];
                    break;
                case 160:
                    buf = Answers.level160[rand.Next(0, Answers.level160.Count - 1)];
                    break;
                case 150:
                    buf = Answers.level150[rand.Next(0, Answers.level150.Count - 1)]; 
                    break;
                case 140:
                    buf = Answers.level140[rand.Next(0, Answers.level140.Count - 1)];
                    break;
                case 130:
                    buf = Answers.level130[rand.Next(0, Answers.level130.Count - 1)];
                    break;
                case 120:
                    buf = Answers.level120[rand.Next(0, Answers.level120.Count - 1)];
                    break;
                case 110:
                    buf = Answers.level110[rand.Next(0, Answers.level110.Count - 1)];
                    break;
                case 100:
                    buf = Answers.level100[rand.Next(0, Answers.level100.Count - 1)];
                    break;
                case 90:
                    buf = Answers.level90[rand.Next(0, Answers.level90.Count - 1)];
                    break;
                case 80:
                    buf = Answers.level80[rand.Next(0, Answers.level80.Count - 1)];
                    break;
                case 70:
                    buf = Answers.level70[rand.Next(0, Answers.level70.Count - 1)];
                    break;
                case 60:
                    buf = Answers.level60[rand.Next(0, Answers.level60.Count - 1)];
                    break;
                case 50:
                    buf = Answers.level50[rand.Next(0, Answers.level50.Count - 1)];
                    break;
                case 40:
                    buf = Answers.level40[rand.Next(0, Answers.level40.Count - 1)];
                    break;
                case 30:
                    buf = Answers.level30[rand.Next(0, Answers.level30.Count - 1)];
                    break;
                case 20:
                    buf = Answers.level20[rand.Next(0, Answers.level20.Count - 1)];
                    break;
                case 10:
                    buf = Answers.level10[rand.Next(0, Answers.level10.Count - 1)];
                    break;
                case 0:
                    buf = Answers.level0[rand.Next(0, Answers.level0.Count - 1)];
                    break;
                default:
                    break;
            }
            return buf;
        }
    }
}
