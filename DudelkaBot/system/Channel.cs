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
using System.Data.SqlClient;

namespace DudelkaBot.system
{
    public class Channel
    {
        #region UserData
            static string userId = "145466944";
            static string OAuth = "k1vf6fr82i4inavo2odnhuaq8d8rz2";
            static string url = "https://im.twitch.tv/v1/messages?on_site=";
        #endregion
        private static Random rand = new Random();
        public static List<string> commands = new List<string>()
        {
            "ТОЛЬКО ДЛЯ МОДЕРАТОРОВ: ",
            "!vote [Тема голосования]:[время в мин]:[variant1,variant2,variantn] (через , без пробелов) - голосование",
            "!advert [время в мин] [кол-во повторов] [объявление] - объявление",
            "!death [+|-|v|value] - счетчик глобальных смертей + - добавить/убавить, v - показать, value - установить значение",
            "!deathbattle [+|-|v|value] - счетчик смертей на боссе + - добавить/убавить(текущий и глобальный вместе), v - показать, value - установить значение",
            "!vkid id[номер страницы в вк] ИЛИ !vkid [сокращение страницы в вк] - установка странницы в вк стриммера, необходимо для работы !music",
            "!qupdate [номер цитаты] [цитата] - принудительная вставка/редактирование цитаты",
            "ДЛЯ МОДЕРАТОРОВ И САБОВ от 6 МЕСЯЦЕВ ПОДПИСКИ.",
            "!quote [+|-] [цитата|номер] - добавить/удалить цитату",
            "@DudelkaBot, /color [Цвет, по стандарту] - установить цвет бота",
            "ДЛЯ ВСЕХ: ",
            "!sexylevel - ваш уровень сексуальности на канале",
            "!date - дата и время сервера",
            "!help - список команд",
            "!members - кол-во сексуалов в чате",
            "!mystat - ваша статистика за все время",
            "!toplist - топ общительных за все время",
            "!citytime - время в Уфе",
            "!music - вывести текущую музыку из страницы в вк стриммера",
            "!quote [номер] - вывести цитату"
        };

        public Status Status = Status.Unknown;
        public Dictionary<string, List<User>> voteResult = new Dictionary<string, List<User>>();
        public bool VoteActive = false;

        private Timer VoteTimer;
        private Timer StreamTimer;
        private Timer QuoteTimer;

        public static IrcClient ircClient;
        public static Dictionary<string, Channel> channels = new Dictionary<string, Channel>();
        public static List<Message> errorListMessages = new List<Message>();
        public static Channel viewChannel;

        #region Patterns
        private static string answerpattern = @"@DudelkaBot, (?<text>.+)";
        private static string subpattern = @".+subscriber\/(?<sub>\d+).+";
        private static string idpattern = @".+user-id=(?<id>\d+);.+";
        #endregion

        #region ReferenceFields
        private static Timer connectTimer = new Timer(CheckConnect, null, 5 * 60000, 5 * 60000);
        private static Regex answerReg = new Regex(answerpattern);
        private static Regex subReg = new Regex(subpattern);
        private static Regex idReg = new Regex(idpattern);
        private static HttpsClient req = new HttpsClient(userId, OAuth, url);
        #endregion

        #region Fields
        private string name;
        private int countMessageForTenMin = 0;
        private int countMessageQuote = 0;
        private int lastqouteindex = 0;
        private int deathBattleCount = 0;
        private string iphost;
        private string userName;
        private string password;
        private int port;
        private int id; 
        #endregion

        public bool IdConnect
        {
            get
            {
                if (ircClient != null)
                    return ircClient.isConnect();
                return false;
            }
        }
        public static HttpsClient Req { get => req; set => req = value; }
        public static Timer ConnectTimer { get => connectTimer; set => connectTimer = value; }
        public static Regex AnswerReg { get => answerReg; set => answerReg = value; }
        public static Regex SubReg { get => subReg; set => subReg = value; }
        public static Regex IdReg { get => idReg; set => idReg = value; }
        public int CountMessageForTenMin { get => countMessageForTenMin; set => countMessageForTenMin = value; }
        public int CountMessageQuote { get => countMessageQuote; set => countMessageQuote = value; }
        public int Lastqouteindex { get => lastqouteindex; set => lastqouteindex = value; }
        public int DeathBattleCount { get => deathBattleCount; set => deathBattleCount = value; }
        public string Iphost { get => iphost; set => iphost = value; }
        public string UserName { get => userName; set => userName = value; }
        public string Password { get => password; set => password = value; }
        public string Name { get => name; set => name = value; }
        public int Port { get => port; set => port = value; }
        public int Id { get => id; set => id = value; }

        public Channel(string channelName, string iphost, int port, string userName, string password)
        { 
            Name = channelName;
            if (!channels.ContainsKey(Name))
                channels.Add(channelName, this);
            this.Iphost = iphost;
            this.Port = port;
            this.Password = password;
            this.UserName = userName;
            using (var db = new ChatContext())
            {
                var chan = db.Channels.SingleOrDefault(a => a.Channel_name == channelName);
                if (chan == null)
                {
                    chan = new Channels(channelName);
                    db.Channels.Add(chan);
                    db.SaveChangesAsync().Wait();
                    Id = chan.Channel_id;
                }
                else
                {
                    Id = chan.Channel_id;
                }
                db.SaveChangesAsync().Wait();

                //findDublicateUsers(db);
            }
            StreamTimer = new Timer(StreamStateUpdate, null, 10* 60000, 10 * 60000);
            QuoteTimer = new Timer(ShowQuote, null, 20 * 60000, 20 * 60000);

        }

        private static void CheckConnect(object obj)
        {
            ircClient.isConnect();
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
                    Console.Write(item.Key.ToString() + " - " + item.Value.ToString() + " - ");
                    foreach (var g in db.Users)
                    {
                        if (g.Username == item.Key)
                            Console.Write(" " + g.Id.ToString());
                    }
                    Console.WriteLine();
                }
            }
        }

        private void ShowQuote(object obj)
        {
            if(CountMessageQuote > 40)
            {
                using (var db = new ChatContext()) {
                    var or = db.Quotes.Where(a => a.Channel_id == Id).ToList();
                    if (or.Count <= 0)
                    {
                        CountMessageQuote = 0;
                        return;
                    }
                    int c = rand.Next(1, or.Count);
                    while(c == Lastqouteindex)
                        c = rand.Next(1, or.Count); ;
                    Lastqouteindex = c;
                    var quot = or.SingleOrDefault(a => a.Number == Lastqouteindex);
                    if (quot != null)
                        ircClient.SendChatBroadcastMessage(string.Format("/me Великая цитата Kappa №{0} : '{1}'", Lastqouteindex, quot.Quote), Name);
                }
            }
            CountMessageQuote = 0;
        }

        public static void Reconnect()
        {
            if (ircClient != null)
            {
                foreach (var item in channels)
                {
                    ircClient.LeaveRoom(item.Key);
                }

                ircClient.Reconnect(Process);

                foreach (var item in channels)
                {
                    ircClient.JoinRoom(item.Key);
                }
            }
        }

        private void StreamStateUpdate(object obj)
        {
            try
            {
                if (CountMessageForTenMin == 0)
                {
                    Status = Status.Offline;
                    using (var db = new ChatContext())
                    {
                        foreach (var item in db.ChannelsUsers.Where(a => a.Active && a.Channel_id == Id))
                        {
                            item.Active = false;
                        }
                        db.SaveChangesAsync().Wait();
                    }
                }
                else
                {
                    Status = Status.Online;
                    CountMessageForTenMin = 0;
                }
            }
            catch (System.InvalidOperationException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Source + ex.Data + ex.Message);
                Console.ResetColor();
                return;
            }
        }

        public void StartShow()
        {
            viewChannel = this;
        }

        public void StopShow()
        {
            viewChannel = null;
        }

        public void Join()
        {
            if (ircClient == null)
            {
                ircClient = new IrcClient(Iphost, Port, UserName, Password);
                ircClient.StartProcess(Process);
            }
            ircClient.JoinRoom(Name);

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

                if(viewChannel != null && currentMessage.Channel == viewChannel.Name && currentMessage.Msg != null)
                    Console.WriteLine(currentMessage.UserName + ": " + currentMessage.Msg);

                if (!currentMessage.Success)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(data);
                    Console.ResetColor();
                    lock (errorListMessages)
                    {
                        if (errorListMessages.Count > 50)
                            errorListMessages.Clear();
                        errorListMessages.Add(currentMessage);
                    }
                    return;
                }
                else if (currentMessage.UserName == "moobot" || currentMessage.UserName == "nightbot")
                    return;

                if (currentMessage.Channel != null)
                {
                    if (channels.ContainsKey(currentMessage.Channel))
                        channels[currentMessage.Channel].Handler(currentMessage);
                }
                else
                {
                    channels.First().Value?.Handler(currentMessage);
                }
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Source + " " + ex.Message + " " + ex.Data);
                Console.ResetColor();
                return;
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

        private void Handler(Message msg)
        {
            try
            {
                using (var db = new ChatContext())
                {
                    switch (msg.Type)
                    {
                        case TypeMessage.JOIN:
                            JoinMessage(msg, db);
                            break;
                        case TypeMessage.PART:
                            PartMessage(msg, db);
                            break;
                        case TypeMessage.MODE:
                            ModeMessage(msg, db);
                            break;
                        case TypeMessage.NAMES:
                            NamesMessage(msg, db);
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
                                        ircClient.SendChatBroadcastMessage("/color " + math.Groups["color"].Value, msg);
                                    break;
                                }
                                else
                                {
                                    math = AnswerReg.Match(msg.Msg);
                                    if (math.Success)
                                    {
                                        ircClient.SendChatMessage(math.Groups["text"].Value, msg);
                                        break;
                                    }
                                }
                            }

                            switch (msg.command)
                            {
                                case Command.vote:
                                    var userVote = db.Users.FirstOrDefault(a => a.Username == msg.UserName);
                                    if (userVote == null)
                                        break;
                                    var chus = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == userVote.Id);

                                    if (chus == null)
                                        break;

                                    if ((chus.Moderator || msg.UserName == "dudelka_krasnaya" || msg.UserName == Name) && msg.VoteActive && !VoteActive)
                                    {
                                        VoteTimer = new Timer(StopVote, msg, msg.Time * 60000, msg.Time * 60000);
                                        lock (voteResult)
                                        {
                                            for (int i = 0; i < msg.variants.Count; i++)
                                            {
                                                voteResult.Add(msg.variants[i], new List<User>());
                                                msg.variants[i] = (i + 1).ToString() + ")" + msg.variants[i];
                                            }
                                            msg.variants.Insert(0, "/me Начинается голосование по теме: ' " + msg.Theme + " ' Время: " + msg.Time.ToString() + "мин." + " Варианты: ");
                                            msg.variants.Add(" Пишите НОМЕР варианта или САМ вариант!.");
                                        }
                                        ircClient.SendChatBroadcastChatMessage(msg.variants, msg);
                                        VoteActive = true;
                                    }
                                    break;
                                case Command.advert:
                                    var userAdvert = db.Users.FirstOrDefault(a => a.Username == msg.UserName);
                                    if (userAdvert == null)
                                        break;
                                    var chu = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == userAdvert.Id);
                                    if (chu == null)
                                        break;
                                    if ((chu.Moderator || msg.UserName == "dudelka_krasnaya" || msg.UserName == Name))
                                    {
                                        Advert advert = new Advert(msg.AdvertTime, msg.AdvertCount, msg.Advert, Name);
                                        ircClient.SendChatMessage("Объявление '" + msg.Advert + "' активировано", msg);
                                    }
                                    break;
                                case Command.citytime:
                                    ircClient.SendChatMessage("Время в Уфе - " + DateTime.Now.AddHours(2).TimeOfDay.ToString().Remove(8), msg);
                                    break;
                                case Command.help:
                                    var u = IdReg.Match(msg.Data);
                                    if (u.Success)
                                    {
                                        SendWhisperMessage(u.Groups["id"].Value, msg.UserName, commands);
                                        //await req.MakePostRequest("hui", 113282518, "POST");
                                        //Console.WriteLine(s);
                                        //ircClient.sendChatWhisperMessage(commands, ulong.Parse(u.Groups["id"].Value), msg);
                                    }
                                    break;
                                case Command.date:
                                    ircClient.SendChatMessage(DateTime.Now.ToString(), msg);
                                    break;
                                case Command.mystat:
                                    var userStat = db.Users.FirstOrDefault(a => a.Username == msg.UserName);
                                    if (userStat == null)
                                        break;
                                    var chusStat = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == userStat.Id);
                                    var math = SubReg.Match(msg.Data);
                                    if (math.Success)
                                        chusStat.CountSubscriptions = int.Parse(math.Groups["sub"].Value) > chusStat.CountSubscriptions ? int.Parse(math.Groups["sub"].Value) : chusStat.CountSubscriptions;
                                    db.SaveChangesAsync().Wait();
                                    ircClient.SendChatMessage("Вы написали " + chusStat.CountMessage.ToString() + " сообщений на канале" + (chusStat.CountSubscriptions > 0 ? ", также вы сексуальны уже " + chusStat.CountSubscriptions.ToString() + " месяца(ев) KappaPride" : ""), msg);
                                    break;
                                case Command.toplist:
                                    List<string> toplist;
                                    if (db.ChannelsUsers.Where(a => a.Channel_id == Id).Count() < 5)
                                        break;
                                    var channelsusers = db.ChannelsUsers.Where(a => a.Channel_id == Id).OrderByDescending(a => a.CountMessage).ToList();
                                    toplist = new List<string>()
                                    {
                                        "Топ 5 самых общительных(сообщения): " +  db.Users.Single(a => a.Id == channelsusers[0].User_id).Username + " = " + channelsusers[0].CountMessage.ToString() + " ,",
                                        db.Users.Single(a => a.Id == channelsusers[1].User_id).Username + " = " + channelsusers[1].CountMessage.ToString() + " ,",
                                        db.Users.Single(a => a.Id == channelsusers[2].User_id).Username + " = " + channelsusers[2].CountMessage.ToString() + " ,",
                                        db.Users.Single(a => a.Id == channelsusers[3].User_id).Username + " = " + channelsusers[3].CountMessage.ToString() + " ,",
                                        db.Users.Single(a => a.Id == channelsusers[4].User_id).Username + " = " + channelsusers[4].CountMessage.ToString(),
                                    };
                                    ircClient.SendChatBroadcastChatMessage(toplist, msg);

                                    break;
                                case Command.sexylevel:

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
                                        db.SaveChangesAsync().Wait();
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
                                    ircClient.SendChatBroadcastMessage(string.Format("Сейчас в чате {0} человек, {1} лучших модеров и не только KappaPride ", db.ChannelsUsers.Where(a => a.Active && a.Channel_id == Id && !a.Moderator).Count(), db.ChannelsUsers.Where(a => a.Active && a.Channel_id == Id && a.Moderator).Count()), msg);
                                    break;
                                case Command.unknown:
                                    lock (errorListMessages)
                                        errorListMessages.Add(msg);
                                    break;
                                case Command.music:
                                    var ch = db.Channels.FirstOrDefault(a => a.Channel_id == Id);
                                    if (ch.VkId as object != null)
                                    {
                                        if (ch.VkId == 0)
                                        {
                                            ircClient.SendChatMessage("Не установлен Vk Id для канала, см. !help", msg);
                                            break;
                                        }
                                        string trackname = Vkontakte.getNameTrack(ch.VkId);
                                        if (string.IsNullOrEmpty(trackname))
                                            ircClient.SendChatMessage("В данный момент музыка не играет :(", msg);
                                        else
                                            ircClient.SendChatMessage("Сейчас играет: " + trackname + " Kreygasm", msg);
                                    }
                                    break;
                                case Command.vkid:
                                    var t = db.Users.FirstOrDefault(a => a.Username == msg.UserName)?.Id;
                                    if (t == null)
                                        break;
                                    var mod = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == t && a.Moderator);
                                    if (mod == null)
                                        break;
                                    var mat = Regex.Match(msg.vkid, @"id(?<id>\d+)$");
                                    if (msg.vkid.Contains("id"))
                                    {
                                        mat = Regex.Match(msg.vkid, @"id(?<id>\d+)$");

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
                                        mat = Regex.Match(msg.vkid, @"(?<screenname>\w+)$");
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
                                    db.SaveChangesAsync().Wait();
                                    break;
                                case Command.death:
                                    var userDeath = db.Users.FirstOrDefault(a => a.Username == msg.UserName);
                                    if (userDeath == null)
                                        break;
                                    var chDeath = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == userDeath.Id);
                                    if (chDeath == null)
                                        break;
                                    if (chDeath.Moderator || msg.UserName == "dudelka_krasnaya" || msg.UserName == Name)
                                    {
                                        var channel = db.Channels.FirstOrDefault(a => a.Channel_name == Name);
                                        int current = channel.DeathCount;
                                        switch (msg.DeathCommand)
                                        {
                                            case "+":
                                                channel.DeathCount++;
                                                break;
                                            case "-":
                                                if (channel.DeathCount > 0)
                                                    channel.DeathCount--;
                                                break;
                                            case "v":
                                                ircClient.SendChatBroadcastMessage(string.Format("/me Умерли всего {0} {1} LUL", channel.DeathCount, Helper.GetDeclension(channel.DeathCount, "раз", "раза", "раз")), Name);
                                                break;
                                            default:
                                                int val;
                                                if (int.TryParse(msg.DeathCommand, out val))
                                                {
                                                    channel.DeathCount = val > 0 ? val : 0;
                                                }
                                                break;
                                        }
                                        if (current != channel.DeathCount)
                                            ircClient.SendChatBroadcastMessage(string.Format("/me Умерли всего {0} {1} FeelsBadMan", channel.DeathCount, Helper.GetDeclension(channel.DeathCount, "раз", "раза", "раз")), Name);
                                        db.SaveChangesAsync().Wait();
                                    }
                                    break;
                                case Command.deathbattle:
                                    var userBattleDeath = db.Users.FirstOrDefault(a => a.Username == msg.UserName);
                                    if (userBattleDeath == null)
                                        break;
                                    var chBattleDeath = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == userBattleDeath.Id);
                                    if (chBattleDeath == null)
                                        break;
                                    if (chBattleDeath.Moderator || msg.UserName == "dudelka_krasnaya" || msg.UserName == Name)
                                    {
                                        var channel = db.Channels.FirstOrDefault(a => a.Channel_name == Name);
                                        int current = DeathBattleCount;
                                        switch (msg.DeathCommand)
                                        {
                                            case "+":
                                                channel.DeathCount++;
                                                DeathBattleCount++;
                                                break;
                                            case "-":
                                                if (channel.DeathCount > 0)
                                                    channel.DeathCount--;
                                                if (DeathBattleCount > 0)
                                                    DeathBattleCount--;
                                                break;
                                            case "v":
                                                ircClient.SendChatBroadcastMessage(string.Format("/me Умерли на боссе {0} {1} FeelsBadMan", DeathBattleCount, Helper.GetDeclension(DeathBattleCount, "раз", "раза", "раз")), Name);
                                                break;
                                            default:
                                                int val;
                                                if (int.TryParse(msg.DeathCommand, out val))
                                                {
                                                    DeathBattleCount = val > 0 ? val : 0;
                                                }
                                                break;
                                        }
                                        if (current != channel.DeathCount)
                                            ircClient.SendChatBroadcastMessage(string.Format("/me Умерли на боссе {0} {1} FeelsBadMan", DeathBattleCount, Helper.GetDeclension(DeathBattleCount, "раз", "раза", "раз")), Name);
                                        db.SaveChangesAsync().Wait();
                                    }
                                    break;
                                case Command.qupdate:
                                    var Userqupdate = db.Users.FirstOrDefault(a => a.Username == msg.UserName);
                                    if ((Userqupdate != null && db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == Userqupdate.Id && a.Moderator) != null) || msg.UserName == "dudelka_krasnaya")
                                    {
                                        int x = msg.quoteNumber;
                                        if (x <= 0)
                                            break;
                                        var uu = db.Quotes.Where(a => a.Channel_id == Id).ToList();

                                        var quo = uu.FirstOrDefault(a => a.Number == x);
                                        if (quo != null)
                                        {
                                            quo.Quote = msg.Quote;
                                            ircClient.SendChatMessage(string.Format("Цитата№{0} - отредактирована", quo.Number), msg);
                                        }
                                        else
                                        {
                                            var o = new Quotes(Id, msg.Quote, DateTime.Now.Date, x);
                                            db.Quotes.Add(o);
                                            db.SaveChangesAsync().Wait();
                                            ircClient.SendChatMessage(string.Format("Цитата№{0} : {1} - добавлена", o.Number, o.Quote), msg);
                                        }
                                        db.SaveChangesAsync().Wait();
                                    }
                                    break;
                                case Command.quote:
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
                                            db.SaveChangesAsync().Wait();
                                        }
                                        if (chusModer.CountSubscriptions >= 6)
                                            Moder = true;
                                    }
                                    switch (msg.quoteOperation)
                                    {
                                        case "+":
                                            if (Moder && !cur.Any(a => a.Quote == msg.Quote))
                                            {
                                                var nu = cur.Count() + 1;
                                                var e = new Quotes(Id, msg.Quote, DateTime.Now, nu);
                                                db.Quotes.Add(e);
                                                db.SaveChangesAsync().Wait();
                                                ircClient.SendChatBroadcastMessage(string.Format("Цитата №{0} : {1} - добавлена", nu, msg.Quote), msg);
                                            }

                                            break;
                                        case "-":
                                            int y = int.Parse(msg.Quote);
                                            var o = cur.FirstOrDefault(a => a.Number == y);

                                            if (Moder && o != null)
                                            {
                                                ircClient.SendChatBroadcastMessage(string.Format("Цитата №{0} : {1} - удалена", y, o.Quote), msg);
                                                db.Quotes.Remove(o);
                                                db.SaveChangesAsync().Wait();
                                            }
                                            break;
                                        default:
                                            if (Moder || chusModer.CountSubscriptions > 6)
                                            {
                                                int q = msg.quoteNumber;
                                                var r = cur.FirstOrDefault(a => a.Number == q);
                                                if (r != null)
                                                    ircClient.SendChatMessage(string.Format("Цитата №{0} от {1:dd/MM/yyyy} : {2}", q, r.Date, r.Quote), msg);
                                            }
                                            break;
                                    }
                                    db.SaveChangesAsync().Wait();
                                    break;
                                default:
                                    lock (errorListMessages)
                                        errorListMessages.Add(msg);
                                    break;
                            }
                            break;
                        case TypeMessage.GLOBALUSERSTATE:
                            break;
                        case TypeMessage.UNKNOWN:
                            break;
                        case TypeMessage.PING:
                            ircClient.PingResponse();
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Source + " " + ex.Message + " " + ex.Data);
                Console.ResetColor();
                return;
            }
            catch (DbUpdateException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Source + " " + ex.Message + " " + ex.Data);
                Console.ResetColor();
                Handler(msg);
                return;
            }
            catch (AggregateException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Source + " " + ex.Message + " " + ex.Data);
                Console.ResetColor();
                return;
            }
            catch (InvalidOperationException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Source + " " + ex.Message + " " + ex.Data);
                Console.ResetColor();
                Handler(msg);
                return;
            }
        }

        private void VoteHandlerMessage(Message msg)
        {
            lock (voteResult)
            {
                if ((voteResult.ContainsKey(msg.Msg) || (msg.Msg.All(char.IsDigit) && int.Parse(msg.Msg) <= voteResult.Count ? true : false)) && !isUserVote(msg))
                {
                    if (msg.Msg.All(char.IsDigit))
                    {
                        voteResult[voteResult.ElementAt(int.Parse(msg.Msg) - 1).Key].Add(new User(msg.UserName));
                    }
                    else
                        voteResult[msg.Msg].Add(new User(msg.UserName));
                }
            }
        }

        private void SubscribeMessage(Message msg, ChatContext db)
        {
            var userNotify = db.Users.FirstOrDefault(a => a.Username == msg.SubscriberName);
            int chussub = 0;
            if (userNotify != null)
            {
                var chus = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == userNotify.Id);

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
                    chus = new ChannelsUsers(userNotify.Id, Id, msg.Subscription) { Active = true };
                    if (msg.Subscription > chus.CountSubscriptions)
                        chus.CountSubscriptions = msg.Subscription;
                    else
                        chus.CountSubscriptions++;
                    db.ChannelsUsers.Add(chus);
                }
                chussub = chus.CountSubscriptions;
            }
            else
            {
                userNotify = new Users(msg.SubscriberName);
                db.Users.Add(userNotify);
                db.SaveChangesAsync().Wait();

                var chus = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == userNotify.Id);

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
                    chus = new ChannelsUsers(userNotify.Id, Id, msg.Subscription) { Active = true };
                    if (msg.Subscription > chus.CountSubscriptions)
                        chus.CountSubscriptions = msg.Subscription;
                    else
                        chus.CountSubscriptions++;
                    db.ChannelsUsers.Add(chus);
                }
                chussub = chus.CountSubscriptions;
            }
            db.SaveChangesAsync().Wait();
            ircClient.SendChatMessage(string.Format("Спасибо за подписку! Добро пожаловать к нам, с {0} - месяцем тебя Kappa {1}", chussub > msg.Subscription ? chussub : msg.Subscription, chussub > msg.Subscription ? "Псс. Я тебя помню, меня не обманешь Kappa , добро пожаловать снова! ":""), msg.SubscriberName, msg);
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
                    if (msg.Subscription > chus.CountSubscriptions)
                        chus.CountSubscriptions = msg.Subscription;
                    else
                        chus.CountSubscriptions++;
                    db.ChannelsUsers.Add(chus);
                }
            }
            else
            {
                userNotice = new Users(msg.SubscriberName);
                db.Users.Add(userNotice);
                db.SaveChangesAsync().Wait();

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
                    if (msg.Subscription > chus.CountSubscriptions)
                        chus.CountSubscriptions = msg.Subscription;
                    else
                        chus.CountSubscriptions++;
                    db.ChannelsUsers.Add(chus);
                }
            }
            db.SaveChangesAsync().Wait();

            var j = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == userNotice.Id);
            if (j != null)
                ircClient.SendChatMessage(string.Format("Спасибо за переподписку! Ты снова с нами, с {0} - месяцем тебя Kappa", j.CountSubscriptions), msg.SubscriberName, msg);
        }

        private void HandlerCountMessages(Message msg, ChatContext db)
        {
            CountMessageForTenMin++;
            CountMessageQuote++;
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
                db.SaveChangesAsync().Wait();

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
            db.SaveChangesAsync().Wait();
        }

        private void NamesMessage(Message msg, ChatContext db)
        {
            foreach (var item in msg.NamesUsers)
            {
                var userNames = db.Users.FirstOrDefault(a => a.Username == item);

                if (userNames != null)
                {
                    var chus = db.ChannelsUsers.Where(p => p.Channel_id == Id).FirstOrDefault(p => p.User_id == userNames.Id);

                    if (chus != null)
                        chus.Active = true;
                    else
                        db.ChannelsUsers.Add(new ChannelsUsers(userNames.Id, Id) { Active = true });
                }
                else
                {
                    userNames = new Users(item);
                    db.Users.Add(userNames);
                    db.SaveChangesAsync().Wait();

                    var chus = db.ChannelsUsers.Where(p => p.Channel_id == Id).FirstOrDefault(p => p.User_id == userNames.Id);

                    if (chus != null)
                        chus.Active = true;
                    else
                        db.ChannelsUsers.Add(new ChannelsUsers(userNames.Id, Id) { Active = true });
                }
                db.SaveChangesAsync().Wait();
            }
        }

        private void ModeMessage(Message msg, ChatContext db)
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
                db.SaveChangesAsync().Wait();

                var chus = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == userMode.Id);

                if (chus != null)
                    chus.Moderator = msg.Sign == "+" ? true : false;
                else
                    chus = new ChannelsUsers(userMode.Id, Id) { Moderator = msg.Sign == "+" ? true : false };
            }

            db.SaveChangesAsync().Wait();
        }

        private void JoinMessage(Message msg, ChatContext db)
        {
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
                db.SaveChangesAsync().Wait();

                var chus = db.ChannelsUsers.Where(p => p.Channel_id == Id).FirstOrDefault(p => p.User_id == user.Id);

                if (chus != null)
                    chus.Active = true;
                else
                    db.ChannelsUsers.Add(new ChannelsUsers(user.Id, Id) { Active = true });
            }
            db.SaveChangesAsync().Wait();
        }

        private void PartMessage(Message msg, ChatContext db)
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
                db.SaveChangesAsync().Wait();

                var chus = db.ChannelsUsers.Where(p => p.Channel_id == Id).FirstOrDefault(p => p.User_id == userPart.Id);

                if (chus != null)
                    chus.Active = false;
                else
                    db.ChannelsUsers.Add(new ChannelsUsers(userPart.Id, Id) { Active = false });
            }
            db.SaveChangesAsync().Wait();
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
                if ((buff + item + "; ").Length < 500)
                    buff += item + "; ";
                else {
                    Req.SendPost(touser_id, buff);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("send "+ username + ": ");
                    Console.WriteLine(buff);
                    Console.ResetColor();
                    buff = item + "; ";
                    Thread.Sleep(700);
                }
            }
            if (!string.IsNullOrEmpty(buff))
            {
                Req.SendPost(touser_id, buff);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("send " + username + ": ");
                Console.WriteLine(buff);
                Console.ResetColor();
            }
            var ig = new Users(username);
            IrcClient.WhisperBlock.Add(ig, new Timer(IrcClient.BlockWhisperCancel, ig, 60000, 60000));
        }

        private static void SendWhisperMessage(string touser_id, string username, string message)
        {
            if (IrcClient.WhisperBlock.Any(a => a.Key.Username == username))
                return;
            if (!string.IsNullOrEmpty(message) && message.Length < 500)
                Req.SendPost(touser_id, message);
            else
                return;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("send " + username + ": ");
            Console.WriteLine(message);
            Console.ResetColor();
            var ig = new Users(username);
            IrcClient.WhisperBlock.Add(ig, new Timer(IrcClient.BlockWhisperCancel, ig, 60000, 60000));
        }

        private void StopVote(object s)
        {
            lock (voteResult)
            {
                VoteActive = false;
                StringBuilder builder = new StringBuilder(voteResult.Count);

                builder.Append($"/me Голосование по теме ' {(s as Message).Theme} ' окончено! Результаты: ");
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
                builder.Append(" Победил - < " + win + " > с результатом в " + max.ToString() + " голосов.");

                ircClient.SendChatBroadcastMessage(builder.ToString(), Name);
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
                    if (ircClient.isConnect())
                    {
                        string s = ircClient.ReadMessage();
                        if (s != null)
                            Task.Run(() => SwitchMessage(s));
                        Thread.Sleep(10);
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    Console.ResetColor();
                    Console.ReadLine();
                    return;
                }
            }
        }

        private string GetLevelMessage(int level)
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
