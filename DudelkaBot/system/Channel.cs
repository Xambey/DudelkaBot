using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

using DudelkaBot.dataBase.model;
using DudelkaBot.vk;
using DudelkaBot.ircClient;
using DudelkaBot.resources;
using DudelkaBot.WebClients;
using System.Data.SqlClient;
using DudelkaBot.Messages;
using DudelkaBot.enums;
using DudelkaBot.Logging;

namespace DudelkaBot.system
{
    public class Channel
    {
        #region UserData
            static string userId = "145466944";
            static string OAuth = "k1vf6fr82i4inavo2odnhuaq8d8rz2";
            static string url = "https://im.twitch.tv/v1/messages?on_site=";
            static string client_id = "58apdkh91ows8rrfnph8ld96d7lqsrc";
        #endregion

        #region ResurcesPaths
        public static string commandsPath = "./resources/commands.txt";

        public static string level0 = "./resources/answers/level0.txt";
        public static string level10 = "./resources/answers/level10.txt";
        public static string level20 = "./resources/answers/level20.txt";
        public static string level30 = "./resources/answers/level30.txt";
        public static string level40 = "./resources/answers/level40.txt";
        public static string level50 = "./resources/answers/level50.txt";
        public static string level60 = "./resources/answers/level60.txt";
        public static string level70 = "./resources/answers/level70.txt";
        public static string level80 = "./resources/answers/level80.txt";
        public static string level90 = "./resources/answers/level90.txt";
        public static string level100 = "./resources/answers/level100.txt";
        public static string level110 = "./resources/answers/level110.txt";
        public static string level120 = "./resources/answers/level120.txt";
        public static string level130 = "./resources/answers/level130.txt";
        public static string level140 = "./resources/answers/level140.txt";
        public static string level150 = "./resources/answers/level150.txt";
        public static string level160 = "./resources/answers/level160.txt";
        public static string level170 = "./resources/answers/level170.txt";
        public static string level180 = "./resources/answers/level180.txt";
        public static string level190 = "./resources/answers/level190.txt";
        public static string level200 = "./resources/answers/level200.txt";
        #endregion

        #region Timers
        private Timer VoteTimer;
        private Timer StreamChatTimer;
        private Timer QuoteTimer;
        private Timer StreamTimer;
        static private int StreamStateChatUpdateTime = 3;
        static private int StreamStateUpdateTime = 1;
        static private int QuoteShowTime = 15;
        static private readonly int CountLimitMessagesForUpdateStreamState = 15;
        static private readonly int countLimitMessagesForShowQuote = 40;
        #endregion

        #region Patterns
        private static string answerpattern = @"@DudelkaBot, (?<text>.+)";
        private static string subpattern = @".+subscriber\/(?<sub>\d+).+";
        private static string idpattern = @".+user-id=(?<id>\d+);.+";
        #endregion

        #region References
        private static Timer connectTimer = new Timer(CheckConnect, null, 5 * 60000, 5 * 60000);
        private static Regex answerReg = new Regex(answerpattern);
        private static Regex subReg = new Regex(subpattern);
        private static Regex idReg = new Regex(idpattern);
        private static HttpsClient req = new HttpsClient(userId, OAuth, url);
        private static IrcClient ircClient;
        private static Dictionary<string, Channel> channels = new Dictionary<string, Channel>();
        private static Channel viewChannel;
        private static List<Message> errorListMessages = new List<Message>();
        private Status statusChat = Status.Offline;
        private Status statusStream = Status.Offline;

        private static Random rand = new Random();
        private static List<string> commands;
        private Dictionary<string, List<User>> voteResult = new Dictionary<string, List<User>>();
        #endregion

        #region Fields
        public int countMessageForUpdateStreamState = 0;
        public int countMessageQuote = 0;
        private string name;
        private int lastqouteindex = 0;
        private int deathBattleCount = 0;
        private static string iphost;
        private static string userName;
        private static string password;
        private static int port;
        private int id;   
        private bool voteActive = false;
        private bool namesHandlerActive = false;
        private static bool activeLog = false;

        #endregion

        #region Properties
        public static HttpsClient Req { get => req; protected set => req = value; }
        public static Timer ConnectTimer { get => connectTimer; protected set => connectTimer = value; }
        public static Regex AnswerReg { get => answerReg; protected set => answerReg = value; }
        public static Regex SubReg { get => subReg; protected set => subReg = value; }
        public static Regex IdReg { get => idReg; protected set => idReg = value; }
        public static IrcClient IrcClient { get => ircClient; protected set => ircClient = value; }
        public static Dictionary<string, Channel> Channels { get => channels; protected set => channels = value; }
        public static Channel ViewChannel { get => viewChannel; protected set => viewChannel = value; }
        public static List<Message> ErrorListMessages { get => errorListMessages; protected set => errorListMessages = value; }
        public Status StatusChat { get => statusChat; protected set => statusChat = value; }
        public static Random Rand { get => rand; protected set => rand = value; }
        public static List<string> Commands { get => commands; protected set => commands = value; }
        public Dictionary<string, List<User>> VoteResult { get => voteResult; protected set => voteResult = value; }
        public bool VoteActive { get => voteActive; protected set => voteActive = value; }
        public int Lastqouteindex { get => lastqouteindex; protected set => lastqouteindex = value; }
        public int DeathBattleCount { get => deathBattleCount; protected set => deathBattleCount = value; }
        public static int Port { get => port; set => port = value; }
        public int Id { get => id; set => id = value; }
        public static string Iphost { get => iphost; set => iphost = value; }
        public static string UserName { get => userName; set => userName = value; }
        public static string Password { get => password; set => password = value; }
        public string Name { get => name; protected set => name = value; }
        public Status StatusStream { get => statusStream; set => statusStream = value; }
        public bool NamesHandlerActive { get => namesHandlerActive; set => namesHandlerActive = value; }
        public static bool ActiveLog { get => activeLog; set => activeLog = value; }
        #endregion

        public Channel(string channelName)
        {
            try
            {
                Name = channelName;
                if (!Channels.ContainsKey(Name))
                    Channels.Add(channelName, this);

                if (Commands == null)
                {
                    Commands = new List<string>(System.IO.File.ReadAllLines(commandsPath));
                }
                using (var db = new ChatContext())
                {
                    var chan = db.Channels.SingleOrDefault(a => a.Channel_name == channelName);
                    if (chan == null)
                    {
                        chan = new Channels(channelName);
                        db.Channels.Add(chan);
                        db.SaveChanges();
                        Id = chan.Channel_id;
                    }
                    else
                    {
                        Id = chan.Channel_id;
                    }
                    db.SaveChanges();

                    //findDublicateUsers(db);
                }
                if (Profiller.Profiller.GetProfileOrDefault(Name) == null)
                    Profiller.Profiller.TryCreateProfile(Name);

                req.GetChannelInfo(Name, client_id);
                Logger.UpdateChannelPaths(Name);
                StreamChatTimer = new Timer(StreamStateChatUpdate, null, StreamStateChatUpdateTime * 60000, StreamStateChatUpdateTime * 60000);
                QuoteTimer = new Timer(ShowQuote, null, QuoteShowTime * 60000, QuoteShowTime * 60000);
                StreamTimer = new Timer(StreamStateUpdate, null, StreamStateUpdateTime * 60000, StreamStateUpdateTime * 60000);
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

        private static void CheckConnect(object obj)
        {
            IrcClient.isConnect();
        }

        private static void FindDublicateUsers(ChatContext db)
        {
            Dictionary<string, int> map = new Dictionary<string, int>();
            foreach (var item in db.Users)
            {
                if (!map.ContainsKey(item.Username))
                {
                    map.Add(item.Username, 1);
                }
                else
                    map[item.Username]++;
            }

            foreach (var item in map)
            {
                if (item.Value > 1)
                {
                    Logger.ShowLineCommonMessage(item.Key.ToString() + " - " + item.Value.ToString() + " - ");
                    foreach (var g in db.Users)
                    {
                        if (g.Username == item.Key)
                            Logger.ShowCommonMessage(" " + g.Id.ToString());
                    }
                }
            }
        }

        private void ShowQuote(object obj)
        {
            if(countMessageQuote > countLimitMessagesForShowQuote)
            {
                using (var db = new ChatContext()) {
                    var or = db.Quotes.Where(a => a.Channel_id == Id).ToList();
                    if (or.Count <= 0)
                    {
                        countMessageQuote = 0;
                        return;
                    }
                    int c = Rand.Next(1, or.Count());
                    while(c == Lastqouteindex)
                        c = Rand.Next(1, or.Count());
                    Lastqouteindex = c;
                    var quot = or.SingleOrDefault(a => a.Number == Lastqouteindex);
                    if (quot != null)
                        IrcClient.SendChatBroadcastMessage(string.Format("/me Великая случайная цитата LUL - {0}, {1:yyyy} #{2} : '{3}'", Name, quot.Date, Lastqouteindex, quot.Quote), Name);
                }
            }
            countMessageQuote = 0;
        }

        public static void Reconnect()
        {
            if (IrcClient != null)
            {
                foreach (var item in Channels)
                {
                    IrcClient.LeaveRoom(item.Key);
                }

                Logger.StopWrite();
                Logger.ShowLineCommonMessage("Соединение разорвано...");
                IrcClient.Reconnect(Process);
                Logger.StartWrite();
                foreach (var item in Channels)
                {
                    item.Value.JoinRoom();
                }
                Logger.ShowLineCommonMessage("Соединение установлено...");
            }
            else
            {
                ircClient = new IrcClient(Iphost, Port, UserName, Password);
                foreach (var item in Channels)
                {
                    IrcClient.JoinRoom(item.Key);
                }
                Logger.ShowLineCommonMessage("Соединение разорвано...");
                Logger.ShowLineCommonMessage("Соединение установлено...");
            }
        }

        private void StreamStateUpdate(object obj)
        {
            try
            {
                var oldstatus = StatusStream;
                var info = req.GetChannelInfo(Name, client_id);
                switch (info.Item1)
                {
                    case Status.Online:
                        StatusStream = Status.Online;
                        break;
                    case Status.Offline:
                        StatusStream = Status.Offline;
                        break;
                    case Status.Unknown:
                        StatusStream = Status.Unknown;
                        Logger.ShowLineCommonMessage("Ошибка загрузки статуса канала!");
                        break;
                    default:
                        break;
                }

                if (oldstatus != StatusStream)
                {
                    if (StatusStream == Status.Offline || StatusChat == Status.Online)
                    {
                        Logger.SaveChannelLog(Name);
                        Logger.SaveCommonLog();
                        Logger.UpdateChannelPaths(Name);
                    }
                    Logger.ShowLineCommonMessage($"Канал {Name} сменил статус на {StatusStream.ToString().ToUpper()}");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Logger.ShowLineCommonMessage(ex.Source + ex.Data + ex.Message);
                if (ex.InnerException != null)
                    Logger.ShowLineCommonMessage(ex.InnerException.Source + ex.InnerException.Data + ex.InnerException.Message);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        private void StreamStateChatUpdate(object obj)
        {
            try
            {
                var oldstatus = StatusChat;
                if (countMessageForUpdateStreamState <= CountLimitMessagesForUpdateStreamState)
                {
                    StatusChat = Status.Offline;
                    using (var db = new ChatContext())
                    {
                        foreach (var item in db.ChannelsUsers.Where(a => a.Active && a.Channel_id == Id))
                        {
                            item.Active = false;
                        }
                        db.SaveChanges();
                    }
                }
                else
                {
                    StatusChat = Status.Online;
                    countMessageForUpdateStreamState = 0;
                }

                if (oldstatus != StatusChat)
                {
                    //if(StatusChat == Status.Offline)
                    //    Logger.UpdateChannelPaths(Name);
                    Logger.ShowLineCommonMessage($"Чат канала {Name} сменил статус на {StatusChat.ToString().ToUpper()}");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Logger.ShowLineCommonMessage(ex.Source + ex.Data + ex.Message);
                if(ex.InnerException != null)
                    Logger.ShowLineCommonMessage(ex.InnerException.Source + ex.InnerException.Data + ex.InnerException.Message);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public void StartShow()
        {
            ViewChannel = this;
            Logger.ShowLineCommonMessage($"Включено отображение чата {Name} ...");
        }

        public void StopShow()
        {
            ViewChannel = null;
            Logger.ShowLineCommonMessage($"Отображение чата отключено...");
        }

        public void JoinRoom()
        {
            if (IrcClient == null)
            {
                IrcClient = new IrcClient(Iphost, Port, UserName, Password);
                IrcClient.StartProcess(Process);
            }
            IrcClient.JoinRoom(Name);
            Logger.ShowLineCommonMessage($"Выполнен вход в комнату: {Name} ...");
        }

        public void LeaveRoom()
        {
            if (IrcClient == null)
            {
                IrcClient = new IrcClient(Iphost, Port, UserName, Password);
                IrcClient.StartProcess(Process);
            }
            IrcClient.LeaveRoom(Name);
            Logger.ShowLineCommonMessage($"Выполнен выход из комнаты: {Name} ...");
        }

        private int GetSexyLevel(string username)
        {
            int level = 0;
            using(var db = new ChatContext())
            {
                var us = db.Users.SingleOrDefault(a => a.Username == username);
                if (us == null)
                    return 0;

                var ID = us.Id;

                var user = db.ChannelsUsers.SingleOrDefault(a => a.User_id == ID && a.Channel_id == Id);
                if (user == null)
                    return 0;

                if (user.CountSubscriptions >= 12)
                    level += 100;
                else if (user.CountSubscriptions >= 10)
                    level += 80;
                else if (user.CountSubscriptions >= 8)
                    level += 70;
                else if (user.CountSubscriptions >= 6)
                    level += 60;
                else if (user.CountSubscriptions >= 4)
                    level += 40;
                else if (user.CountSubscriptions >= 3)
                    level += 30;
                else if (user.CountSubscriptions >= 2)
                    level += 20;
                else if (user.CountSubscriptions >= 1)
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

        private static void SwitchMessage(string data)
        {
            try
            {
                Message currentMessage = new Message(data);

                if(ViewChannel != null && currentMessage.Channel == ViewChannel.Name && currentMessage.Msg != null)
                    Logger.ShowLineChannelMessage(currentMessage.UserName,currentMessage.Msg, currentMessage.Channel);
                else if(!currentMessage.Success)
                {
                    Console.ForegroundColor = ConsoleColor.Green;

                    if(ActiveLog)
                        Logger.ShowLineCommonMessage(data);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    //lock (ErrorListMessages)
                    //{
                    //    if (ErrorListMessages.Count > 50)
                    //        ErrorListMessages.Clear();
                    //    ErrorListMessages.Add(currentMessage);
                    //}
                    return;
                }
                else if (currentMessage.UserName == "moobot" || currentMessage.UserName == "nightbot")
                    return;
                else if (ViewChannel != null && currentMessage.Channel != ViewChannel.Name && currentMessage.Msg != null)
                    Logger.WriteLineMessage(currentMessage.UserName,currentMessage.Msg, currentMessage.Channel);

                if (currentMessage.Channel != null && Channels.ContainsKey(currentMessage.Channel))
                {
                        Channels[currentMessage.Channel].Handler(currentMessage);
                }
                else
                {
                    Channels.First().Value?.Handler(currentMessage);
                }
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Logger.ShowLineCommonMessage(ex.Source + " " + ex.Message + " " + ex.Data);
                Console.ForegroundColor = ConsoleColor.Gray;
                return;
            }
        }

        private bool isUserVote(Message msg)
        {
            lock (VoteResult)
            {
                foreach (var item in VoteResult)
                {
                    if (item.Value.All(a => a.UserName != msg.UserName) == false)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private void Handler(Message msg)
        {
            try
            {
                using (var db = new ChatContext())
                {
                    switch (msg.Type)
                    {
                        case TypeMessage.JOIN:
                            HandlerJoinMessage(msg, db);
                            break;
                        case TypeMessage.PART:
                            HandlerPartMessage(msg, db);
                            break;
                        case TypeMessage.MODE:
                            HandlerModeMessage(msg, db);
                            break;
                        case TypeMessage.NAMES:
                            HandlerNamesMessage(msg, db);
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
                            UsernoticeMessage(msg, db);
                            break;
                        case TypeMessage.Tags:
                            break;
                        case TypeMessage.PRIVMSG:
                            HandlerCountMessages(msg, db);

                            if (msg.UserName == "twitchnotify")
                            {
                                SubscribeMessage(msg, db);
                                break;
                            }
                            else if (VoteActive)
                            {
                                VoteHandlerMessage(msg);
                                break;
                            }
                            else
                            {
                                var math = Regex.Match(msg.Msg, @"\/color (?<color>\w+)$");
                                if (math.Success)
                                {
                                    var u = db.Users.FirstOrDefault(a => a.Username == msg.UserName);
                                    if (u == null)
                                        break;
                                    var g = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == u.Id);
                                    if (g == null)
                                        break;
                                    if (g.Moderator || msg.UserName == "dudelka_krasnaya")
                                        IrcClient.SendChatBroadcastMessage("/color " + math.Groups["color"].Value, msg);
                                    break;
                                }
                                else
                                {
                                    math = AnswerReg.Match(msg.Msg);
                                    if (math.Success)
                                    {
                                        IrcClient.SendChatMessage(math.Groups["text"].Value, msg);
                                        break;
                                    }
                                }
                            }

                            switch (msg.Command)
                            {
                                case Command.vote:
                                    var pr = Profiller.Profiller.GetProfileOrDefault(Name);
                                    if (pr == null)
                                        Profiller.Profiller.TryCreateProfile(Name);
                                    if (pr != null && pr.Vote == 0)
                                        break;
                                    var userVote = db.Users.FirstOrDefault(a => a.Username == msg.UserName);
                                    if (userVote == null)
                                        break;
                                    var chus = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == userVote.Id);

                                    if (chus == null)
                                        break;

                                    if ((chus.Moderator || msg.UserName == "dudelka_krasnaya" || msg.UserName == Name) && msg.VoteActive && !VoteActive)
                                    {
                                        VoteTimer = new Timer(StopVote, msg, msg.Time * 60000, msg.Time * 60000);
                                        lock (VoteResult)
                                        {
                                            for (int i = 0; i < msg.Variants.Count; i++)
                                            {
                                                VoteResult.Add(msg.Variants[i], new List<User>());
                                                msg.Variants[i] = (i + 1).ToString() + ")" + msg.Variants[i];
                                            }
                                            msg.Variants.Insert(0, "/me Начинается голосование по теме: ' " + msg.Theme + " ' Время: " + msg.Time.ToString() + "мин." + " Варианты: ");
                                            msg.Variants.Add(" Пишите НОМЕР варианта или САМ вариант!.");
                                        }
                                        IrcClient.SendChatBroadcastChatMessage(msg.Variants, msg);
                                        VoteActive = true;
                                    }
                                    break;
                                case Command.advert:
                                    var pro = Profiller.Profiller.GetProfileOrDefault(Name);
                                    if (pro == null)
                                        Profiller.Profiller.TryCreateProfile(Name);
                                    if (pro != null && pro.Advert == 0)
                                        break;
                                    var userAdvert = db.Users.FirstOrDefault(a => a.Username == msg.UserName);
                                    if (userAdvert == null)
                                        break;
                                    var chu = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == userAdvert.Id);
                                    if (chu == null)
                                        break;
                                    if ((chu.Moderator || msg.UserName == "dudelka_krasnaya" || msg.UserName == Name))
                                    {
                                        Advert advert = new Advert(msg.AdvertTime, msg.AdvertCount, msg.Advert, Name);
                                        IrcClient.SendChatMessage("Объявление '" + msg.Advert + "' активировано", msg);
                                    }
                                    break;
                                case Command.citytime:
                                    var prof = Profiller.Profiller.GetProfileOrDefault(Name);
                                    if (prof == null)
                                        Profiller.Profiller.TryCreateProfile(Name);
                                    if (prof != null && prof.Citytime == 0)
                                        break;
                                    IrcClient.SendChatMessage("Время в Уфе - " + DateTime.Now.AddHours(2).TimeOfDay.ToString().Remove(8), msg);
                                    break;
                                case Command.help:
                                    var profi = Profiller.Profiller.GetProfileOrDefault(Name);
                                    if (profi == null)
                                        Profiller.Profiller.TryCreateProfile(Name);
                                    if (profi != null && profi.Help == 0)
                                        break;
                                    var u = IdReg.Match(msg.Data);
                                    if (u.Success)
                                    {
                                        SendWhisperMessage(u.Groups["id"].Value, msg.UserName, Commands);
                                    }
                                    break;
                                case Command.date:
                                    var profil = Profiller.Profiller.GetProfileOrDefault(Name);
                                    if (profil == null)
                                        Profiller.Profiller.TryCreateProfile(Name);
                                    if (profil != null && profil.Date == 0)
                                        break;
                                    IrcClient.SendChatMessage("Время в москве: " + DateTime.Now.TimeOfDay.ToString().Remove(8), msg);
                                    break;
                                case Command.mystat:
                                    var profile = Profiller.Profiller.GetProfileOrDefault(Name);
                                    if (profile == null)
                                        Profiller.Profiller.TryCreateProfile(Name);
                                    if (profile != null && profile.Mystat == 0)
                                        break;

                                    var userStat = db.Users.FirstOrDefault(a => a.Username == msg.UserName);
                                    if (userStat == null)
                                        break;
                                    var chusStat = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == userStat.Id);
                                    var math = SubReg.Match(msg.Data);
                                    if (math.Success)
                                        chusStat.CountSubscriptions = int.Parse(math.Groups["sub"].Value) > chusStat.CountSubscriptions ? int.Parse(math.Groups["sub"].Value) : chusStat.CountSubscriptions;
                                    db.SaveChanges();
                                    IrcClient.SendChatMessage("Вы написали " + chusStat.CountMessage.ToString() + " сообщений на канале" + (chusStat.CountSubscriptions > 0 ? ", также вы подписаны уже " + chusStat.CountSubscriptions.ToString() + " месяца(ев) FeelsLitMan " : ""), msg);
                                    break;
                                case Command.toplist:
                                    var profiler = Profiller.Profiller.GetProfileOrDefault(Name);
                                    if (profiler == null)
                                        Profiller.Profiller.TryCreateProfile(Name);
                                    if (profiler != null && profiler.Toplist == 0)
                                        break;
                                    
                                    if (db.ChannelsUsers.Where(a => a.Channel_id == Id).Count() < 5)
                                        break;
                                    var channelsusers = db.ChannelsUsers.Where(a => a.Channel_id == Id).OrderByDescending(a => a.CountMessage).ToList();
                                    List<string> toplist = new List<string>()
                                    {
                                        "Топ 5 самых общительных(сообщения): " +  db.Users.Single(a => a.Id == channelsusers[0].User_id).Username + " = " + channelsusers[0].CountMessage.ToString() + " ,",
                                        db.Users.Single(a => a.Id == channelsusers[1].User_id).Username + " = " + channelsusers[1].CountMessage.ToString() + " ,",
                                        db.Users.Single(a => a.Id == channelsusers[2].User_id).Username + " = " + channelsusers[2].CountMessage.ToString() + " ,",
                                        db.Users.Single(a => a.Id == channelsusers[3].User_id).Username + " = " + channelsusers[3].CountMessage.ToString() + " ,",
                                        db.Users.Single(a => a.Id == channelsusers[4].User_id).Username + " = " + channelsusers[4].CountMessage.ToString(),
                                    };
                                    IrcClient.SendChatBroadcastChatMessage(toplist, msg);

                                    break;
                                case Command.uptime:
                                    var pru = Profiller.Profiller.GetProfileOrDefault(Name);
                                    if (pru == null)
                                        Profiller.Profiller.TryCreateProfile(Name);
                                    if (pru != null && pru.Uptime == 0)
                                        break;

                                    if (StatusStream != Status.Online)
                                        break;
                                    var value = req.GetChannelInfo(Name, client_id);
                                    if (value.Item1 != Status.Online)
                                        break;
                                    var time = DateTime.Now - value.Item3; 
                                    IrcClient.SendChatMessage($"Чатику хорошо уже {time.Hours} {Helper.GetDeclension(time.Hours,"час","часа","часов")}, {time.Minutes} {Helper.GetDeclension(time.Hours, "минута", "минуты", "минуты")}, {time.Seconds} {Helper.GetDeclension(time.Hours, "секунды", "секунд", "секунды")} Kreygasm " , msg);
                                    break;
                                case Command.sexylevel:
                                    var prus = Profiller.Profiller.GetProfileOrDefault(Name);
                                    if (prus == null)
                                        Profiller.Profiller.TryCreateProfile(Name);
                                    if (prus != null && prus.Sexylevel == 0)
                                        break;
                                    var m = SubReg.Match(msg.Data);
                                    if (msg.Channel == "dariya_willis")
                                        break;
                                    if (m.Success)
                                    {
                                        var usLevel = db.Users.FirstOrDefault(a => a.Username == msg.UserName);
                                        if (usLevel == null)
                                            break;
                                        var chan = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == usLevel.Id);
                                        int val = int.Parse(m.Groups["sub"].Value);
                                        if (chan.CountSubscriptions < val)
                                            chan.CountSubscriptions = val;
                                        db.SaveChanges();
                                    }

                                    int level = GetSexyLevel(msg.UserName);

                                    if (msg.UserName == Name)
                                        level = 200;
                                    m = IdReg.Match(msg.Data);
                                    if (m.Success)
                                    {
                                        SendWhisperMessage(m.Groups["id"].Value, msg.UserName, "Ваш уровень сексуальности " + level.ToString() + " из 200" + ", вы настолько сексуальны, что: " + GetLevelMessage(level));
                                        //ircClient.sendChatWhisperMessage("Ваш уровень сексуальности " + level.ToString() + " из 200" + ", вы настолько сексуальны, что: " + getLevelMessage(level), ulong.Parse(m.Groups["id"].Value), msg);
                                    }

                                    break;
                                case Command.members:
                                    var prum = Profiller.Profiller.GetProfileOrDefault(Name);
                                    if (prum == null)
                                        Profiller.Profiller.TryCreateProfile(Name);
                                    if (prum != null && prum.Members == 0)
                                        break;
                                    IrcClient.SendChatBroadcastMessage(string.Format("Сейчас в чате {0} активных человек, {1} лучших модеров и не только Kappa ", db.ChannelsUsers.Where(a => a.Active && a.Channel_id == Id && !a.Moderator).Count(), db.ChannelsUsers.Where(a => a.Active && a.Channel_id == Id && a.Moderator).Count()), msg);
                                    break;
                                case Command.unknown:
                                    lock (ErrorListMessages)
                                        ErrorListMessages.Add(msg);
                                    break;
                                case Command.viewers:
                                    var prumt = Profiller.Profiller.GetProfileOrDefault(Name);
                                    if (prumt == null)
                                        Profiller.Profiller.TryCreateProfile(Name);
                                    if (prumt != null && prumt.Viewers == 0)
                                        break;
                                    var la = req.GetChannelInfo(Name, client_id);
                                    if (la.Item1 != Status.Online)
                                        break;
                                    IrcClient.SendChatMessage($"Сейчас стрим смотрит {la.Item2} человек Jebaited ", msg);
                                    break;
                                case Command.music:
                                    var prumi = Profiller.Profiller.GetProfileOrDefault(Name);
                                    if (prumi == null)
                                        Profiller.Profiller.TryCreateProfile(Name);
                                    if (prumi != null && prumi.Music == 0)
                                        break;
                                    if (StatusStream != Status.Online)
                                        break;
                                    var ch = db.Channels.FirstOrDefault(a => a.Channel_id == Id);
                                    
                                    if (ch.VkId as object != null)
                                    {
                                        if (ch.VkId == 0)
                                        {
                                            IrcClient.SendChatMessage("Не установлен Vk Id для канала, см. !help", msg);
                                        }
                                        string trackname = Vkontakte.getNameTrack(ch.VkId);
                                        if (string.IsNullOrEmpty(trackname))
                                        {
                                            if (ch.DjId as object != null)
                                            {
                                                if (ch.DjId == 0)
                                                {
                                                    Thread.Sleep(400);
                                                    IrcClient.SendChatMessage("Не установлен DjId для канала, см. !help", msg);
                                                    break;
                                                }
                                                string g = req.GetMusicFromTwitchDJ(ch.DjId.ToString()).Result;
                                                if (string.IsNullOrEmpty(g))
                                                {
                                                    IrcClient.SendChatMessage("В данный момент музыка нигде не играет FeelsBadMan !", msg);
                                                }
                                                else
                                                {
                                                    IrcClient.SendChatMessage(g, msg);
                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            IrcClient.SendChatMessage("Сейчас в VK играет: " + trackname + " Kreygasm", msg);
                                            break;
                                        }
                                    }
                                    
                                    break;
                                case Command.djid:
                                    var prom = Profiller.Profiller.GetProfileOrDefault(Name);
                                    if (prom == null)
                                        Profiller.Profiller.TryCreateProfile(Name);
                                    if (prom != null && prom.Djid == 0)
                                        break;

                                    var pe = db.Users.FirstOrDefault(a => a.Username == msg.UserName)?.Id;
                                    if (pe == null)
                                        break;
                                    var mode = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == pe && a.Moderator);
                                    if (mode == null && msg.UserName != "dudelka_krasnaya") 
                                        break;
                                    var chih = db.Channels.FirstOrDefault(a => a.Channel_name == Name);
                                    if (chih == null)
                                        break;
                                    int v;
                                    int.TryParse(msg.Djid, out v);
                                    if (v == 0)
                                        break;
                                    chih.DjId = v;
                                    ircClient.SendChatMessage("DjId сохранено!", msg);
                                    db.SaveChanges();
                                    break;
                                case Command.vkid:
                                    var promi = Profiller.Profiller.GetProfileOrDefault(Name);
                                    if (promi == null)
                                        Profiller.Profiller.TryCreateProfile(Name);
                                    if (promi != null && promi.Vkid == 0)
                                        break;

                                    var t = db.Users.FirstOrDefault(a => a.Username == msg.UserName)?.Id;
                                    if (t == null)
                                        break;
                                    var mod = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == t && a.Moderator);
                                    if (mod == null)
                                        break;
                                    var mat = Regex.Match(msg.Vkid, @"id(?<id>\d+)$");
                                    if (msg.Vkid.Contains("id"))
                                    {
                                        mat = Regex.Match(msg.Vkid, @"id(?<id>\d+)$");

                                        if (mat.Success)
                                        {
                                            var d = long.Parse(mat.Groups["id"].Value);
                                            if (Vkontakte.userExist(d))
                                                db.Channels.FirstOrDefault(a => a.Channel_name == Name).VkId = (int)d;
                                            else break;
                                        }
                                    }
                                    else
                                    {
                                        mat = Regex.Match(msg.Vkid, @"(?<screenname>\w+)$");
                                        if (mat.Success)
                                        {
                                            long? vkid = Vkontakte.getUserId(mat.Groups["screenname"].Value);
                                            if (vkid != null)
                                                db.Channels.FirstOrDefault(a => a.Channel_name == Name).VkId = (int)vkid;
                                            else
                                                break;
                                        }
                                        else
                                            break;
                                    }
                                    db.SaveChanges();
                                    break;
                                case Command.counter:
                                    var promic = Profiller.Profiller.GetProfileOrDefault(Name);
                                    if (promic == null)
                                        Profiller.Profiller.TryCreateProfile(Name);
                                    if (promic != null && promic.Counter == 0)
                                        break;
                                    var userCounter = db.Users.FirstOrDefault(a => a.Username == msg.UserName);
                                    if (userCounter == null)
                                        break;
                                    var chCounter = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == userCounter.Id);
                                    if (chCounter == null)
                                        break;
                                    if (chCounter.Moderator || msg.UserName == "dudelka_krasnaya" || msg.UserName == Name)
                                    {
                                        var channel = db.Channels.FirstOrDefault(a => a.Channel_name == msg.Channel);
                                        if (channel == null)
                                            break;
                                        var counters = db.Counters.Where(a => a.Channel_id == channel.Channel_id);

                                        if(string.IsNullOrEmpty(msg.Sign))
                                        {
                                            if (counters == null)
                                                break;
                                            var j = IdReg.Match(msg.Data);
                                            if (j.Success)
                                            {
                                                SendWhisperMessage(j.Groups["id"].Value, msg.UserName, counters.Select(a => a.Counter_name + " ").ToList());
                                            }
                                        }

                                        switch(msg.Sign)
                                        { 
                                            case "+":
                                                if (counters != null)
                                                {
                                                    var y = counters.FirstOrDefault(a => a.Counter_name == msg.NewName);
                                                    if (y == null)
                                                    {
                                                        db.Counters.Add(new Counters(Id, msg.NewName));
                                                        db.SaveChanges();
                                                    }
                                                }
                                                else
                                                {
                                                    db.Counters.Add(new Counters(Id, msg.NewName));
                                                    db.SaveChanges();
                                                }
                                                IrcClient.SendChatBroadcastMessage($"Добавлен новый счетчик {msg.NewName}", msg);
                                                break;
                                            case "-":
                                                if (counters == null)
                                                    break;
                                                var p = counters.FirstOrDefault(a => a.Counter_name == msg.NewName);
                                                if (p != null)
                                                {
                                                    db.Counters.Remove(p);
                                                    db.SaveChanges();
                                                    IrcClient.SendChatBroadcastMessage($"Cчетчик {msg.NewName} удален", msg);
                                                }
                                                break;
                                            default:
                                                break;
                                        }
                                            
                                    }                              
                                    break;
                                case Command.existedcounter:
                                    var promict = Profiller.Profiller.GetProfileOrDefault(Name);
                                    if (promict == null)
                                        Profiller.Profiller.TryCreateProfile(Name);
                                    if (promict != null && promict.Counter == 0)
                                        break;
                                    var exCounter = db.Users.FirstOrDefault(a => a.Username == msg.UserName);
                                    if (exCounter == null)
                                        break;
                                    var chexCounter = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == exCounter.Id);
                                    if (chexCounter == null)
                                        break;
                                    if (chexCounter.Moderator || msg.UserName == "dudelka_krasnaya" || msg.UserName == Name)
                                    {
                                        var channel = db.Channels.FirstOrDefault(a => a.Channel_name == msg.Channel);
                                        if (channel == null)
                                            break;
                                        var counters = db.Counters.Where(a => a.Channel_id == channel.Channel_id);
                                        if (counters == null)
                                            break;

                                        var y = counters.FirstOrDefault(a => a.Counter_name == msg.NewName);
                                        if (y == null)
                                            break;
                                        var oldvalue = y.Count;
                                        switch (msg.Sign)
                                        {
                                            case "+":
                                                y.Count++;
                                                db.SaveChanges();
                                                break;
                                            case "-":
                                                y.Count = y.Count > 0 ? y.Count - 1 : 0;
                                                db.SaveChanges();
                                                break;
                                            case "v":
                                                IrcClient.SendChatBroadcastMessage(string.Format("/me {0}: Умерли всего {1} {2} LUL", y.Counter_name ,y.Count, Helper.GetDeclension(y.Count, "раз", "раза", "раз")), Name);
                                                break;
                                            default:
                                                int val;
                                                if (int.TryParse(msg.Sign, out val))
                                                {
                                                    y.Count = val > 0 ? val : 0;
                                                    db.SaveChanges();
                                                }
                                                break;
                                        }
                                        if(oldvalue != y.Count)
                                            IrcClient.SendChatBroadcastMessage(string.Format("/me {0}: Умерли всего {1} {2} FeelsBadMan", y.Counter_name, y.Count, Helper.GetDeclension(y.Count, "раз", "раза", "раз")), Name);
                                    }
                                    break;
                                case Command.qupdate:
                                    var promis = Profiller.Profiller.GetProfileOrDefault(Name);
                                    if (promis == null)
                                        Profiller.Profiller.TryCreateProfile(Name);
                                    if (promis != null && promis.Qupdate == 0)
                                        break;

                                    var Userqupdate = db.Users.FirstOrDefault(a => a.Username == msg.UserName);
                                    if ((Userqupdate != null && db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == Userqupdate.Id && a.Moderator) != null) || msg.UserName == "dudelka_krasnaya")
                                    {
                                        int x = msg.QuoteNumber;
                                        if (x <= 0)
                                            break;
                                        var uu = db.Quotes.Where(a => a.Channel_id == Id).ToList();

                                        var quo = uu.FirstOrDefault(a => a.Number == x);
                                        if (quo != null)
                                        {
                                            quo.Quote = msg.Quote;
                                            if (msg.Date != default(DateTime))
                                                quo.Date = msg.Date;
                                            IrcClient.SendChatMessage(string.Format("Цитата№{0} - отредактирована", quo.Number), msg);
                                        }
                                        else
                                        {

                                            var o = new Quotes(Id, msg.Quote, msg.Date != default(DateTime) ? msg.Date : DateTime.Now.Date, x);
                                            db.Quotes.Add(o);
                                            db.SaveChanges();
                                            IrcClient.SendChatMessage(string.Format("Цитата№{0} : {1} - добавлена", o.Number, o.Quote), msg);
                                        }
                                        db.SaveChanges();
                                    }
                                    break;
                                case Command.quote:
                                    var prome = Profiller.Profiller.GetProfileOrDefault(Name);
                                    if (prome == null)
                                        Profiller.Profiller.TryCreateProfile(Name);
                                    if (prome != null && prome.Quote == 0)
                                        break;

                                    ChannelsUsers chusModer = null;
                                    var cur = db.Quotes.Where(a => a.Channel_id == Id).ToList();
                                    if (msg.Msg == "!quote")
                                    {
                                        var f = IdReg.Match(msg.Data);
                                        if (f.Success)
                                        {
                                            SendWhisperMessage(f.Groups["id"].Value, msg.UserName, new List<string>(cur.Select(a => string.Format("{0}: '{1}'", a.Number, a.Quote))));
                                            //ircClient.sendChatWhisperMessage(new List<string>(cur.Select(a => string.Format("{0}: '{1}'", a.Number, a.Quote))), ulong.Parse(f.Groups["id"].Value), msg);
                                        }
                                        break;
                                    }
                                    bool Moder;
                                    if (msg.UserName == "dudelka_krasnaya")
                                        Moder = true;
                                    else
                                    {
                                        var userModer = db.Users.FirstOrDefault(a => a.Username == msg.UserName);
                                        if (userModer == null)
                                            break;
                                        chusModer = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == userModer.Id);
                                        if (chusModer == null)
                                            break;
                                        Moder = chusModer.Moderator;
                                        var l = SubReg.Match(msg.Data);
                                        if (l.Success)
                                        {
                                            int b = int.Parse(l.Groups["sub"].Value);
                                            if (b > chusModer.CountSubscriptions)
                                                chusModer.CountSubscriptions = b;
                                            db.SaveChanges();
                                        }
                                        if (chusModer.CountSubscriptions >= 6)
                                            Moder = true;
                                    }
                                    switch (msg.QuoteOperation)
                                    {
                                        case "+":
                                            if (Moder && !cur.Any(a => a.Quote == msg.Quote))
                                            {
                                                var nu = cur.Count() + 1;
                                                var e = new Quotes(Id, msg.Quote, msg.Date != null ? msg.Date : DateTime.Now, nu);
                                                db.Quotes.Add(e);
                                                db.SaveChanges();
                                                IrcClient.SendChatBroadcastMessage(string.Format("Цитата №{0} : {1} - добавлена", nu, msg.Quote), msg);
                                            }

                                            break;
                                        case "-":
                                            int y = int.Parse(msg.Quote);
                                            var o = cur.FirstOrDefault(a => a.Number == y);

                                            if (Moder && o != null)
                                            {
                                                IrcClient.SendChatBroadcastMessage(string.Format("Цитата №{0} : {1} - удалена", y, o.Quote), msg);
                                                db.Quotes.Remove(o);
                                                db.SaveChanges();
                                            }
                                            break;
                                        default:
                                            if (Moder || chusModer.CountSubscriptions > 6)
                                            {
                                                int q = msg.QuoteNumber;
                                                var r = cur.FirstOrDefault(a => a.Number == q);
                                                if (r != null)
                                                    IrcClient.SendChatMessage(string.Format("\"{0}\" - {1}, {2:yyyy} цитата #{3}", r.Quote, Name, r.Date, q), msg);
                                                //ircClient.SendChatMessage(string.Format("Цитата №{0} от {1:dd/MM/yyyy} : {2}", q, r.Date, r.Quote), msg);
                                            }
                                            break;
                                    }
                                    db.SaveChanges();
                                    break;
                                default:
                                    lock (ErrorListMessages)
                                        ErrorListMessages.Add(msg);
                                    break;
                            }
                            break;
                        case TypeMessage.GLOBALUSERSTATE:
                            break;
                        case TypeMessage.UNKNOWN:
                            break;
                        case TypeMessage.PING:
                            IrcClient.PingResponse();
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Logger.ShowLineCommonMessage(ex.Source + " " + ex.Message + " " + ex.Data + " " + ex.StackTrace);
                if(ex.InnerException != null)
                    Logger.ShowLineCommonMessage(ex.InnerException.Source + " " + ex.InnerException.Message + " " + ex.InnerException.Data + " " + ex.InnerException.StackTrace);
                Console.ForegroundColor = ConsoleColor.Gray;
                return;
            }
        }

        private void VoteHandlerMessage(Message msg)
        {
            lock (VoteResult)
            {
                if ((VoteResult.ContainsKey(msg.Msg) || (msg.Msg.All(char.IsDigit) && int.Parse(msg.Msg) <= VoteResult.Count ? true : false)) && !isUserVote(msg))
                {
                    if (msg.Msg.All(char.IsDigit))
                    {
                        VoteResult[VoteResult.ElementAt(int.Parse(msg.Msg) - 1).Key].Add(new User(msg.UserName));
                    }
                    else
                        VoteResult[msg.Msg].Add(new User(msg.UserName));
                }
            }
        }

        private void SubscribeMessage(Message msg, ChatContext db)
        {
            if (msg.Data.Contains("while\\syou\\swere\\saway") || msg.Data.Contains("while you were away"))
                return;
            var userNotify = db.Users.FirstOrDefault(a => a.Username == msg.SubscriberName);
            int chussub = 0;
            if (userNotify != null)
            {
                var chus = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == userNotify.Id);
                if (chus != null)
                {
                    chussub = chus.CountSubscriptions;
                    chus.Active = true;
                    if (msg.Subscription > chus.CountSubscriptions)
                        chus.CountSubscriptions = msg.Subscription;
                    else
                        chus.CountSubscriptions++;
                }
                else
                {
                    chus = new ChannelsUsers(userNotify.Id, Id, msg.Subscription) { Active = true };
                    db.ChannelsUsers.Add(chus);
                }
                
            }
            else
            {
                userNotify = new Users(msg.SubscriberName);
                db.Users.Add(userNotify);
                db.SaveChanges();

                var chus = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == userNotify.Id);
                if (chus != null)
                {
                    chussub = chus.CountSubscriptions;
                    chus.Active = true;
                    if (msg.Subscription > chus.CountSubscriptions)
                        chus.CountSubscriptions = msg.Subscription;
                    else
                        chus.CountSubscriptions++;
                }
                else
                {
                    chus = new ChannelsUsers(userNotify.Id, Id, msg.Subscription) { Active = true };
                    db.ChannelsUsers.Add(chus);
                }
                
            }
            db.SaveChanges();

            var p = Profiller.Profiller.GetProfileOrDefault(Name);
            if (p == null)
            {
                Profiller.Profiller.TryCreateProfile(Name);
                p = Profiller.Profiller.GetProfileOrDefault(Name);
            }

            ircClient.SendChatBroadcastMessage(p.GetRandomSubAnswer().Replace("month", chussub > msg.Subscription ? chussub.ToString() : msg.Subscription.ToString()).Replace("nick", msg.SubscriberName), Name);
            //IrcClient.SendChatMessage(string.Format("Спасибо за подписку! Добро пожаловать к нам, с {0} - месяцем тебя Kappa {1}", chussub > msg.Subscription ? chussub : msg.Subscription, chussub > msg.Subscription ? "Псс. Я тебя помню, меня не обманешь Kappa , добро пожаловать снова! ":""), msg.SubscriberName, msg);
        }

        private void UsernoticeMessage(Message msg, ChatContext db)
        {
            var userNotice = db.Users.FirstOrDefault(a => a.Username == msg.SubscriberName);

            if (userNotice != null)
            {
                var chus = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == userNotice.Id);

                if (chus != null)
                {
                    chus.Active = true;
                    if (msg.Subscription > chus.CountSubscriptions)
                        chus.CountSubscriptions = msg.Subscription;
                    else
                        chus.CountSubscriptions++;
                }
                else
                {
                    chus = new ChannelsUsers(userNotice.Id, Id, msg.Subscription) { Active = true };
                    db.ChannelsUsers.Add(chus);
                }
            }
            else
            {
                userNotice = new Users(msg.SubscriberName);
                db.Users.Add(userNotice);
                db.SaveChanges();

                var chus = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == userNotice.Id);

                if (chus != null)
                {
                    chus.Active = true;
                    if (msg.Subscription > chus.CountSubscriptions)
                        chus.CountSubscriptions = msg.Subscription;
                    else
                        chus.CountSubscriptions++;
                }
                else
                {
                    chus = new ChannelsUsers(userNotice.Id, Id, msg.Subscription) { Active = true };
                    db.ChannelsUsers.Add(chus);
                }
            }
            db.SaveChanges();

            var j = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == userNotice.Id);
            if (j != null) {
                var p = Profiller.Profiller.GetProfileOrDefault(Name);
                if (p == null) {
                    Profiller.Profiller.TryCreateProfile(Name);
                    p = Profiller.Profiller.GetProfileOrDefault(Name);
                }

                ircClient.SendChatBroadcastMessage(p.GetRandomResubAnswer().Replace("month", j.CountSubscriptions.ToString()).Replace("nick", msg.SubscriberName), Name);
                //ircClient.SendChatMessage(string.Format("Спасибо за переподписку! Ты снова с нами, с {0} - месяцем тебя Kappa", j.CountSubscriptions), msg.SubscriberName, msg);
            }
        }

        private void HandlerCountMessages(Message msg, ChatContext db)
        {
            Interlocked.Increment(ref countMessageForUpdateStreamState);
            Interlocked.Increment(ref countMessageQuote);
            var userPRIVMSG = db.Users.FirstOrDefault(a => a.Username == msg.UserName);

            if (userPRIVMSG != null)
            {
                var chus = db.ChannelsUsers.Where(p => p.Channel_id == Id).FirstOrDefault(p => p.User_id == userPRIVMSG.Id);
                if (chus != null)
                    chus.Active = true;
                else
                {
                    chus = new ChannelsUsers(userPRIVMSG.Id, Id) { Active = true, CountMessage = 0 };
                    db.ChannelsUsers.Add(chus);
                }
                chus.CountMessage++;
            }
            else
            {
                userPRIVMSG = new Users(msg.UserName);
                db.Users.Add(userPRIVMSG);
                db.SaveChanges();

                var chus = db.ChannelsUsers.Where(p => p.Channel_id == Id).FirstOrDefault(p => p.User_id == userPRIVMSG.Id);
                if (chus != null)
                    chus.Active = true;
                else
                {
                    chus = new ChannelsUsers(userPRIVMSG.Id, Id) { Active = true, CountMessage = 0 };
                    db.ChannelsUsers.Add(chus);
                }
                chus.CountMessage++;
            }
            db.SaveChanges();
        }

        private void HandlerNamesMessage(Message msg, ChatContext db)
        {
            NamesHandlerActive = true;
            foreach (var item in msg.NamesUsers)
            {
                var userNames = db.Users.FirstOrDefault(a => a.Username == item);

                if (userNames != null)
                {
                    lock (userNames)
                    {
                        var chus = db.ChannelsUsers.Where(p => p.Channel_id == Id).FirstOrDefault(p => p.User_id == userNames.Id);

                        if (chus != null)
                        {
                            lock (chus)
                            {
                                chus.Active = true;
                            }
                        }
                        else
                        {
                            db.ChannelsUsers.Add(new ChannelsUsers(userNames.Id, Id) { Active = true });
                        }
                    }
                }
                else
                {
                    userNames = new Users(item);
                    db.Users.Add(userNames);
                    db.SaveChanges();

                    var chus = db.ChannelsUsers.Where(p => p.Channel_id == Id).FirstOrDefault(p => p.User_id == userNames.Id);

                    if (chus != null)
                    {
                        lock (chus)
                        {
                            chus.Active = true;
                        }
                    }
                    else
                        db.ChannelsUsers.Add(new ChannelsUsers(userNames.Id, Id) { Active = true });
                }
                db.SaveChanges();
                Thread.Sleep(100);
            }
            namesHandlerActive = false;
        }

        private void HandlerModeMessage(Message msg, ChatContext db)
        {
            var userMode = db.Users.FirstOrDefault(a => a.Username == msg.UserName);

            if (userMode != null)
            {
                var chus = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == userMode.Id);

                if (chus != null)
                    chus.Moderator = msg.Sign == "+" ? true : false;
                else
                    chus = new ChannelsUsers(userMode.Id, Id) { Moderator = msg.Sign == "+" ? true : false };
            }
            else
            {
                userMode = new Users(msg.UserName);
                db.Users.Add(userMode);
                db.SaveChanges();

                var chus = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == userMode.Id);

                if (chus != null)
                    chus.Moderator = msg.Sign == "+" ? true : false;
                else
                    chus = new ChannelsUsers(userMode.Id, Id) { Moderator = msg.Sign == "+" ? true : false };
            }

            db.SaveChanges();
        }

        private void HandlerJoinMessage(Message msg, ChatContext db)
        {
            while (namesHandlerActive == true)
                Thread.Sleep(100);
            var user = db.Users.FirstOrDefault(a => a.Username == msg.UserName);

            if (user != null)
            {
                var chus = db.ChannelsUsers.Where(p => p.Channel_id == Id).FirstOrDefault(p => p.User_id == user.Id);

                if (chus != null)
                    chus.Active = true;
                else
                    db.ChannelsUsers.Add(new ChannelsUsers(user.Id, Id) { Active = true });
            }
            else
            {
                user = new Users(msg.UserName);
                db.Users.Add(user);
                db.SaveChanges();

                var chus = db.ChannelsUsers.Where(p => p.Channel_id == Id).FirstOrDefault(p => p.User_id == user.Id);

                if (chus != null)
                    chus.Active = true;
                else
                    db.ChannelsUsers.Add(new ChannelsUsers(user.Id, Id) { Active = true });
            }
            db.SaveChanges();
        }

        private void HandlerPartMessage(Message msg, ChatContext db)
        {
            var userPart = db.Users.FirstOrDefault(a => a.Username == msg.UserName);

            if (userPart != null)
            {
                var chus = db.ChannelsUsers.Where(p => p.Channel_id == Id).FirstOrDefault(p => p.User_id == userPart.Id);

                if (chus != null)
                    chus.Active = false;
                else
                    db.ChannelsUsers.Add(new ChannelsUsers(userPart.Id, Id) { Active = false });
            }
            else
            {
                userPart = new Users(msg.UserName);
                db.Users.Add(userPart);
                db.SaveChanges();

                var chus = db.ChannelsUsers.Where(p => p.Channel_id == Id).FirstOrDefault(p => p.User_id == userPart.Id);

                if (chus != null)
                    chus.Active = false;
                else
                    db.ChannelsUsers.Add(new ChannelsUsers(userPart.Id, Id) { Active = false });
            }
            db.SaveChanges();
        }

        private static void SendWhisperMessage(string touser_id, string username, List<string> message)
        {
            if (IrcClient.WhisperBlock.Any(a => a.Key.Username == username))
                return;
            if (message.Count <= 0)
                return;
            string buff = "";
            foreach (var item in message)
            {
                if ((buff + item + "; ").Length < 490)
                    buff += item + "; ";
                else {
                    Req.SendPostWhisperMessage(touser_id, buff);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    if (ActiveLog)
                    {
                        Logger.ShowCommonMessage("send " + username + ": ");
                        Logger.ShowLineCommonMessage(buff);
                    }
                    Console.ForegroundColor = ConsoleColor.Gray;
                    buff = item + "; ";
                    Thread.Sleep(700);
                }
            }
            if (!string.IsNullOrEmpty(buff))
            {
                Req.SendPostWhisperMessage(touser_id, buff);
                Console.ForegroundColor = ConsoleColor.Yellow;
                if (ActiveLog)
                {
                    Logger.ShowCommonMessage("send " + username + ": ");
                    Logger.ShowLineCommonMessage(buff);
                }
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            var ig = new Users(username);
            IrcClient.WhisperBlock.Add(ig, new Timer(IrcClient.BlockWhisperCancel, ig, 60000, 60000));
        }

        private static void SendWhisperMessage(string touser_id, string username, string message)
        {
            if (IrcClient.WhisperBlock.Any(a => a.Key.Username == username))
                return;
            if (!string.IsNullOrEmpty(message) && message.Length < 500)
                Req.SendPostWhisperMessage(touser_id, message);
            else
                return;
            Console.ForegroundColor = ConsoleColor.Yellow;
            if (ActiveLog)
            {
                Logger.ShowCommonMessage("send " + username + ": ");
                Logger.ShowLineCommonMessage(message);
            }
            Console.ForegroundColor = ConsoleColor.Gray;
            var ig = new Users(username);
            IrcClient.WhisperBlock.Add(ig, new Timer(IrcClient.BlockWhisperCancel, ig, 60000, 60000));
        }

        private void StopVote(object s)
        {
            lock (VoteResult)
            {
                VoteActive = false;
                StringBuilder builder = new StringBuilder(VoteResult.Count);

                builder.Append($"/me Голосование по теме ' {(s as Message).Theme} ' окончено! Результаты: ");
                string win = VoteResult.First().Key;
                int max = VoteResult.First().Value.Count;
                foreach (var item in VoteResult)
                {
                    int current = item.Value.Count;
                    if (max < current)
                    {
                        max = current;
                        win = item.Key;
                    }
                    builder.Append(item.Key + " - " + current + ",");
                }
                builder.Append(" Победил - < " + win + " > с результатом в " + max.ToString() + " голосов.");

                IrcClient.SendChatBroadcastMessage(builder.ToString(), Name);
                VoteResult.Clear();
                VoteTimer.Dispose();
            }
        }

        private static void Process()
        {
            while (true)
            {
                try
                {
                    if (ircClient == null)
                        ircClient = new IrcClient(Iphost, Port, UserName, Password);

                    ircClient.isConnect();
                    string s = ircClient.ReadMessage();
                    if (!string.IsNullOrEmpty(s))
                    {
                        Task.Run(() => SwitchMessage(s));
                    }

                    Thread.Sleep(5);
                }
                catch (AggregateException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Logger.ShowLineCommonMessage(ex.Data + " " + ex.Message + " " + ex.StackTrace);
                    if (ex.InnerException != null)
                        Logger.ShowLineCommonMessage(ex.InnerException.Data + ex.InnerException.Message + ex.InnerException.StackTrace);
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Logger.ShowLineCommonMessage(ex.Data + " " + ex.Message + " " + ex.StackTrace);
                    if (ex.InnerException != null)
                        Logger.ShowLineCommonMessage(ex.InnerException.Data + ex.InnerException.Message + ex.InnerException.StackTrace);
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
            }
        }

        private string GetLevelMessage(int level)
        {
            string buf = "";
            
            switch (level)
            {
                case 200:
                    buf = Answers.level200[Rand.Next(0, Answers.level200.Count())];
                    break;
                case 190:
                    buf = Answers.level190[Rand.Next(0, Answers.level190.Count())]; 
                    break;
                case 180:
                    buf = Answers.level180[Rand.Next(0, Answers.level180.Count())];
                    break;
                case 170:
                    buf = Answers.level170[Rand.Next(0, Answers.level170.Count())];
                    break;
                case 160:
                    buf = Answers.level160[Rand.Next(0, Answers.level160.Count())];
                    break;
                case 150:
                    buf = Answers.level150[Rand.Next(0, Answers.level150.Count())]; 
                    break;
                case 140:
                    buf = Answers.level140[Rand.Next(0, Answers.level140.Count())];
                    break;
                case 130:
                    buf = Answers.level130[Rand.Next(0, Answers.level130.Count())];
                    break;
                case 120:
                    buf = Answers.level120[Rand.Next(0, Answers.level120.Count())];
                    break;
                case 110:
                    buf = Answers.level110[Rand.Next(0, Answers.level110.Count())];
                    break;
                case 100:
                    buf = Answers.level100[Rand.Next(0, Answers.level100.Count())];
                    break;
                case 90:
                    buf = Answers.level90[Rand.Next(0, Answers.level90.Count())];
                    break;
                case 80:
                    buf = Answers.level80[Rand.Next(0, Answers.level80.Count())];
                    break;
                case 70:
                    buf = Answers.level70[Rand.Next(0, Answers.level70.Count())];
                    break;
                case 60:
                    buf = Answers.level60[Rand.Next(0, Answers.level60.Count())];
                    break;
                case 50:
                    buf = Answers.level50[Rand.Next(0, Answers.level50.Count())];
                    break;
                case 40:
                    buf = Answers.level40[Rand.Next(0, Answers.level40.Count())];
                    break;
                case 30:
                    buf = Answers.level30[Rand.Next(0, Answers.level30.Count())];
                    break;
                case 20:
                    buf = Answers.level20[Rand.Next(0, Answers.level20.Count())];
                    break;
                case 10:
                    buf = Answers.level10[Rand.Next(0, Answers.level10.Count())];
                    break;
                case 0:
                    buf = Answers.level0[Rand.Next(0, Answers.level0.Count())];
                    break;
                default:
                    break;
            }
            return buf;
        }
    }
}
