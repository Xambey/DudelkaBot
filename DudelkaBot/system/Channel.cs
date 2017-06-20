using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;
using DudelkaBot.dataBase.model;
using DudelkaBot.vk;
using DudelkaBot.ircClient;
using DudelkaBot.resources;
using DudelkaBot.WebClients;
using DudelkaBot.Messages;
using DudelkaBot.enums;
using DudelkaBot.Logging;
using System.IO;
using System.Collections.Concurrent;

namespace DudelkaBot.system
{
    public class Channel
    {
        #region UserData
        //code: "4fbtdkbbt2vnab2ithpyimuhpvfblb"
        static string userId = "145466944";
        static string commonOAuth = "nti707m9f3py96px8apkvixa5iuifr";// "k1vf6fr82i4inavo2odnhuaq8d8rz2";
        static string url = "https://im.twitch.tv/v1/messages?on_site=";
        static string client_id = "11datgm6whw4p5kgt96h1chveoi4vo";//"11datgm6whw4p5kgt96h1chveoi4vo";
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
        private Timer CheckSubscriptions;
        private Timer ShowChangedMusicTimer;
        private static Timer connectTimer = new Timer(CheckConnect, null, 1 * 60000, 1 * 60000);
        static private int StreamStateChatUpdateTime = 3;
        static private int StreamStateUpdateTime = 1;
        static private int CheckStateSubscriptionsTime = 10000;
        static private int CheckMusicChangedTime = 20000;
        static private int QuoteShowTime = 10;
        static private readonly int CountLimitMessagesForUpdateStreamState = 15;
        static private readonly int countLimitMessagesForShowQuote = 40;
        #endregion

        #region Patterns
        private static string answerpattern = @"(dudelkabot|DudelkaBot)[, ]*(?<text>.+)";
        private static string subpattern = @".+subscriber\/(?<sub>\d+).+";
        private static string idpattern = @".+user-id=(?<id>\d+);.+";
        #endregion

        #region References
        private static Regex answerReg = new Regex(answerpattern);
        private static Regex subReg = new Regex(subpattern);
        private static Regex idReg = new Regex(idpattern);
        private static HttpsClient httpClient = new HttpsClient(userId, commonOAuth, url);
        private static IrcClient ircClient;
        private static Dictionary<string, Channel> channels = new Dictionary<string, Channel>();
        private static Channel viewChannel;
        private static List<Message> errorListMessages = new List<Message>();
        private Status statusChat = Status.Unknown;
        private Status statusStream = Status.Unknown;
        private List<int> NumbersOfQuotations = new List<int>();

        private static Random rand = new Random();
        private static List<string> commands;
        private ConcurrentDictionary<string, ConcurrentBag<User>> voteResult = new ConcurrentDictionary<string, ConcurrentBag<User>>(); 
        private static List<string> ball8 = new List<string>(File.ReadLines("./resources/8ball.txt"));
        private System.Collections.Concurrent.ConcurrentBag<string> subscribedList = new System.Collections.Concurrent.ConcurrentBag<string>();
        private static Dictionary<string, int> lastNetSubscribers = new Dictionary<string, int>();
        #endregion

        #region Fields
        private string Client_ID;
        private string OAuth;
        private int oldnumber8ball = 0;
        public int countMessageForUpdateStreamState = 0;
        public int countMessageQuote = 0;
        private string name;
        private int deathBattleCount = 0;
        private static string iphost;
        private static string userName;
        private static string password;
        private static int port;
        private int id;
        private string netId;
        private bool voteActive = false;
        private bool namesHandlerActive = false;
        private static bool activeLog = false;
        private StatusBot statusBotOnChannel = StatusBot.Active;
        private string lastMusic;
        private ChatMode _chatMode = ChatMode.none;
        private TimeSpan _chatModeInterval = TimeSpan.Zero;
        #endregion

        #region Properties
        public static HttpsClient Req { get => httpClient; protected set => httpClient = value; }
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
        public ConcurrentDictionary<string, ConcurrentBag<User>> VoteResult { get => voteResult; protected set => voteResult = value; }
        public bool VoteActive { get => voteActive; protected set => voteActive = value; }
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
        public StatusBot StatusBotOnChannel { get => statusBotOnChannel; set => statusBotOnChannel = value; }
        public string NetId { get => netId; set => netId = value; }
        public string LastMusic { get => lastMusic; set => lastMusic = value; }
        public ChatMode ChatMode { get => _chatMode; set => _chatMode = value; }
        public TimeSpan ChatModeInterval { get => _chatModeInterval; set => _chatModeInterval = value; }
        #endregion

        #region CommonChatCommands

        private void CommandWakeUp(ChatContext db, Message msg)
        {
            var wake = Profiller.Profiller.GetProfileOrDefault(Name);
            if (wake == null)
                Profiller.Profiller.TryCreateProfile(Name);
            var userwake = db.Users.FirstOrDefault(a => a.Username == msg.UserName);
            if (userwake == null)
                return;
            var chuwake = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == userwake.Id);
            if (chuwake == null)
                return;
            if ((chuwake.Moderator || msg.UserName == "dudelka_krasnaya" || msg.UserName == Name) && StatusBotOnChannel != StatusBot.Active)
            {
                StatusBotOnChannel = StatusBot.Active;
                IrcClient.SendChatBroadcastMessage("/me Мммммм...ну еще 5 минуточек...*трясет* ну ладно, ладно, встаю я...Уже поспать не дают... ResidentSleeper ", Name);
            }
        }

        private void CommandSleep(ChatContext db, Message msg)
        {
            var sleep = Profiller.Profiller.GetProfileOrDefault(Name);
            if (sleep == null)
                Profiller.Profiller.TryCreateProfile(Name);
            var usersleep = db.Users.FirstOrDefault(a => a.Username == msg.UserName);
            if (usersleep == null)
                return;
            var chusleep = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == usersleep.Id);
            if (chusleep == null)
                return;
            if ((chusleep.Moderator || msg.UserName == "dudelka_krasnaya" || msg.UserName == Name) && StatusBotOnChannel != StatusBot.Sleep)
            {
                IrcClient.SendChatBroadcastMessage("/me Ну я не хочу спаааааать, еще 5 минуточек...Zzz ResidentSleeper ", Name);
                StatusBotOnChannel = StatusBot.Sleep;
            }
        }

        private void CommandDiscord(ChatContext db, Message msg)
        {
            var dis = Profiller.Profiller.GetProfileOrDefault(Name);
            if (dis == null)
                Profiller.Profiller.TryCreateProfile(Name);
            if (dis != null && dis.Help == 0)
                return;
            var ud = IdReg.Match(msg.Data);
            if (ud.Success)
            {
                SendWhisperMessage(ud.Groups["id"].Value, msg.UserName, "Ссылка на канал в discord: https://discordapp.com/invite/fsofbsadw");
            }
        }

        private void CommandReconnect(ChatContext db, Message msg)
        {
            var rec = Profiller.Profiller.GetProfileOrDefault(Name);
            if (rec == null)
                Profiller.Profiller.TryCreateProfile(Name);
            var userRec = db.Users.FirstOrDefault(a => a.Username == msg.UserName);
            if (userRec == null)
                return;
            var churec = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == userRec.Id);
            if (churec == null)
                return;
            if ((churec.Moderator || msg.UserName == "dudelka_krasnaya" || msg.UserName == Name))
            {
                IrcClient.SendChatBroadcastMessage("/me Перезагружаюсь... MrDestructoid", Name);
                Reconnect();
            }
        }

        private void CommandBall(ChatContext db, Message msg)
        {
            var ball = Profiller.Profiller.GetProfileOrDefault(Name);
            if (ball == null)
                Profiller.Profiller.TryCreateProfile(Name);
            if (ball.Ball8 == 0)
                return;
            int n = Rand.Next(0, ball8.Count);
            while (n == oldnumber8ball)
                n = Rand.Next(0, ball8.Count);
            IrcClient.SendChatMessage(ball8[n], msg, ChatModeInterval);
            oldnumber8ball = n;
        }

        private void CommandVoteLite(ChatContext db, Message msg)
        {
            var prl = Profiller.Profiller.GetProfileOrDefault(Name);
            if (prl == null)
                Profiller.Profiller.TryCreateProfile(Name);
            if (prl != null && prl.Vote == 0)
                return;
            var userVoteLite = db.Users.FirstOrDefault(a => a.Username == msg.UserName);
            if (userVoteLite == null)
                return;
            var chusl = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == userVoteLite.Id);

            if (chusl == null)
                return;

            if ((chusl.Moderator || msg.UserName == "dudelka_krasnaya" || msg.UserName == Name) && msg.VoteActive && !VoteActive)
            {
                VoteTimer = new Timer(StopVote, msg, 35000, 35000);
                for (int i = 0; i < msg.Variants.Count; i++)
                {
                    VoteResult.TryAdd(msg.Variants[i], new ConcurrentBag<User>());
                    //msg.Variants[i] = (i + 1).ToString() + ")" + msg.Variants[i];
                }
                //msg.Variants.Insert(0, "/me Начинается голосование (30 сек) Squid1 Squid2 DuckerZ Squid3 Squid4 "/* + " ВАРИАНТЫ: "*/);
                //msg.Variants.Add(" Пишите НОМЕР варианта !" /*или САМ вариант!*/ + " SwiftRage  SwiftRage SwiftRage");
                
                VoteActive = true;
                IrcClient.SendChatBroadcastChatMessage(/*msg.Variants*/ new List<string>() { "/me Начинается голосование (30 сек) Squid1 Squid2 DuckerZ Squid3 Squid4 ", "Пишите НОМЕР варианта ! SwiftRage  SwiftRage SwiftRage" }, msg);
            }
        }

        private void StopVote(object s)
        {
            VoteActive = false;
            StringBuilder builder = new StringBuilder(VoteResult.Count);
            if ((s as Message).Theme != null)
                builder.Append($"/me Голосование по теме ' {(s as Message).Theme} ' окончено!");
            else
                builder.Append($"/me Голосование окончено! SwiftRage Голосование окончено! SwiftRage Голосование окончено! SwiftRage Голосование окончено! SwiftRage Голосование окончено! SwiftRage Голосование окончено! SwiftRage Голосование окончено! SwiftRage Голосование окончено! SwiftRage Голосование окончено! SwiftRage ");
            IrcClient.SendChatBroadcastMessage(builder.ToString(), Name);
            builder.Clear();
            Thread.Sleep(2000);
            string win = VoteResult.First().Key;
            int max = VoteResult.First().Value.Count;
            builder.Append("РЕЗУЛЬТАТЫ: ");
            foreach (var item in VoteResult)
            {
                int current = item.Value.Count;
                if (max < current)
                {
                    max = current;
                    win = item.Key;
                }
                builder.Append(item.Key + " [ " + current + " ] , ");
            }
            if (VoteResult.Count(a => a.Value.Count == max) > 1)
                builder.Append(" ПОБЕДИЛИ с ОДИНАКОВЫМ результатом - < " + string.Join(", ", VoteResult.Where(a => a.Value.Count == max).Select(a => a.Key)) + " > с результатом в " + max.ToString() + " голосов.");
            else
                builder.Append(" ПОБЕДИЛ - < " + win + " > с результатом в [ " + max.ToString() + " ] голосов.");

            IrcClient.SendChatBroadcastMessage("#1 " + builder.ToString(), Name);
            Thread.Sleep(3000);
            IrcClient.SendChatBroadcastMessage("#2 " + builder.ToString(), Name);
            Thread.Sleep(3000);
            IrcClient.SendChatBroadcastMessage("#3 " + builder.ToString(), Name);
            VoteResult.Clear();
            VoteTimer.Dispose();
        }

        private void CommandVote(ChatContext db, Message msg)
        {
            var pr = Profiller.Profiller.GetProfileOrDefault(Name);
            if (pr == null)
                Profiller.Profiller.TryCreateProfile(Name);
            if (pr != null && pr.Vote == 0)
                return;
            var userVote = db.Users.FirstOrDefault(a => a.Username == msg.UserName);
            if (userVote == null)
                return;
            var chus = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == userVote.Id);

            if (chus == null)
                return;

            if ((chus.Moderator || msg.UserName == "dudelka_krasnaya" || msg.UserName == Name) && msg.VoteActive && !VoteActive)
            {
                VoteTimer = new Timer(StopVote, msg, msg.Time * 60000, msg.Time * 60000);
                lock (VoteResult)
                {
                    for (int i = 0; i < msg.Variants.Count; i++)
                    {
                        VoteResult.TryAdd(msg.Variants[i], new ConcurrentBag<User>());
                        msg.Variants[i] = (i + 1).ToString() + ")" + msg.Variants[i];
                    }
                    msg.Variants.Insert(0, "/me Начинается голосование по теме: ' " + msg.Theme + " ' Время: " + msg.Time.ToString() + "мин." + " Варианты: ");
                    msg.Variants.Add(" Пишите НОМЕР варианта или САМ вариант!.");
                }
                IrcClient.SendChatBroadcastChatMessage(msg.Variants, msg);
                VoteActive = true;
            }
        }

        private void CommandAdvert(ChatContext db, Message msg)
        {
            var pro = Profiller.Profiller.GetProfileOrDefault(Name);
            if (pro == null)
                Profiller.Profiller.TryCreateProfile(Name);
            if (pro != null && pro.Advert == 0)
                return;
            var userAdvert = db.Users.FirstOrDefault(a => a.Username == msg.UserName);
            if (userAdvert == null)
                return;
            var chu = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == userAdvert.Id);
            if (chu == null)
                return;
            if ((chu.Moderator || msg.UserName == "dudelka_krasnaya" || msg.UserName == Name))
            {
                Advert advert = new Advert(msg.AdvertTime, msg.AdvertCount, msg.Advert, Name);
                IrcClient.SendChatMessage("Объявление '" + msg.Advert + "' активировано", msg);
            }
        }

        private void CommandStreamerTime(ChatContext db, Message msg)
        {
            var prof = Profiller.Profiller.GetProfileOrDefault(Name);
            if (prof == null)
                Profiller.Profiller.TryCreateProfile(Name);
            if (prof != null && prof.Streamertime == 0)
                return;
            //var info = httpClient.GetWeatherInTown("ufa");
            //IrcClient.SendChatMessage("Время в Уфе - " + DateTime.Now.AddHours(2).TimeOfDay.ToString().Remove(8) + " " + (info != null ? info.Item1 + ", " + info.Item2 : " Погода сейчас недоступна :("), msg);
            IrcClient.SendChatMessage("Время у УФЕ - " + DateTime.Now.AddHours(2).TimeOfDay.ToString().Remove(8), msg, ChatModeInterval);
        }

        private void CommandHelp(ChatContext db, Message msg)
        {
            var profi = Profiller.Profiller.GetProfileOrDefault(Name);
            if (profi == null)
                Profiller.Profiller.TryCreateProfile(Name);
            if (profi != null && profi.Help == 0)
                return;
            var u = IdReg.Match(msg.Data);
            if (u.Success)
            {
                //SendWhisperMessage(u.Groups["id"].Value, msg.UserName, Commands);
                SendWhisperMessage(u.Groups["id"].Value, msg.UserName, "Список комманд бота здесь: https://github.com/Xambey/DudelkaBot/wiki/Commands");
            }
        }

        private void CommandMoscowTime(ChatContext db, Message msg)
        {
            var profil = Profiller.Profiller.GetProfileOrDefault(Name);
            if (profil == null)
                Profiller.Profiller.TryCreateProfile(Name);
            if (profil != null && profil.Moscowtime == 0)
                return;
            //var inf = httpClient.GetWeatherInTown("moskva");
            //IrcClient.SendChatMessage("Время в москве: " + DateTime.Now.TimeOfDay.ToString().Remove(8) + " " + (inf != null ? inf.Item1 + ", " + inf.Item2 : " Погода сейчас недоступна :("), msg);
            IrcClient.SendChatMessage("Время в москве: " + DateTime.Now.TimeOfDay.ToString().Remove(8), msg, ChatModeInterval);
        }

        private void CommandMyStat(ChatContext db, Message msg)
        {
            var profile = Profiller.Profiller.GetProfileOrDefault(Name);
            if (profile == null)
                Profiller.Profiller.TryCreateProfile(Name);
            if (profile != null && profile.Mystat == 0)
                return;

            var userStat = db.Users.FirstOrDefault(a => a.Username == msg.UserName);
            if (userStat == null)
                return;
            var chusStat = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == userStat.Id);
            var math = SubReg.Match(msg.Data);
            if (math.Success)
                chusStat.CountSubscriptions = int.Parse(math.Groups["sub"].Value) > chusStat.CountSubscriptions ? int.Parse(math.Groups["sub"].Value) : chusStat.CountSubscriptions;
            db.SaveChanges();
            IrcClient.SendChatMessage("Вы написали " + chusStat.CountMessage.ToString() + " сообщений на канале CoolCat  " + (chusStat.CountSubscriptions > 0 ? ", также вы подписаны уже " + chusStat.CountSubscriptions.ToString() + " месяца(ев) TakeNRG " : ""), msg, ChatModeInterval);
        }

        private void CommandTopList(ChatContext db, Message msg)
        {
            var profiler = Profiller.Profiller.GetProfileOrDefault(Name);
            if (profiler == null)
                Profiller.Profiller.TryCreateProfile(Name);
            if (profiler != null && profiler.Toplist == 0)
                return;

            if (db.ChannelsUsers.Where(a => a.Channel_id == Id).Count() < 5)
                return;
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
        }

        private void CommandUpTime(ChatContext db, Message msg)
        {
            var pru = Profiller.Profiller.GetProfileOrDefault(Name);
            if (pru == null)
                Profiller.Profiller.TryCreateProfile(Name);
            if (pru != null && pru.Uptime == 0)
                return;

            if (StatusStream != Status.Online)
                return;
            var value = httpClient.GetChannelInfo(Name, client_id);
            if (value.Item1 != Status.Online)
                return;
            var time = DateTime.Now - value.Item3;
            IrcClient.SendChatMessage($"Чатику хорошо уже {time.Hours} {Helper.GetDeclension(time.Hours, "час", "часа", "часов")}, {time.Minutes} {Helper.GetDeclension(time.Hours, "минута", "минуты", "минуты")}, {time.Seconds} {Helper.GetDeclension(time.Hours, "секунды", "секунд", "секунды")} Kreygasm ", msg, ChatModeInterval);
        }

        private void CommandSexyLevel(ChatContext db, Message msg)
        {
            //var prus = Profiller.Profiller.GetProfileOrDefault(Name);
            //if (prus == null)
            //    Profiller.Profiller.TryCreateProfile(Name);
            //if (prus != null && prus.Sexylevel == 0)
            //    return;
            var m = SubReg.Match(msg.Data);
            if (msg.Channel == "dariya_willis")
                return;
            if (m.Success)
            {
                var usLevel = db.Users.FirstOrDefault(a => a.Username == msg.UserName);
                if (usLevel == null)
                    return;
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
        }

        private int GetSexyLevel(string username)
        {
            int level = 0;
            using (var db = new ChatContext())
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

        private void CommandMembers(ChatContext db, Message msg)
        {
            var prum = Profiller.Profiller.GetProfileOrDefault(Name);
            if (prum == null)
                Profiller.Profiller.TryCreateProfile(Name);
            if (prum != null && prum.Members == 0)
                return;
            var inf = httpClient.GetCountChattersAndModerators(Name);
            IrcClient.SendChatMessage(string.Format("Сейчас в чате {0} активных человек, {1} лучших модеров и не только Kappa ", /*db.ChannelsUsers.Where(a => a.Active && a.Channel_id == Id && !a.Moderator).Count()*/ inf.Item1, /*db.ChannelsUsers.Where(a => a.Active && a.Channel_id == Id && a.Moderator).Count()*/ inf.Item2), msg, ChatModeInterval);
        }

        private void CommandViewers(ChatContext db, Message msg)
        {
            var prumt = Profiller.Profiller.GetProfileOrDefault(Name);
            if (prumt == null)
                Profiller.Profiller.TryCreateProfile(Name);
            if (prumt != null && prumt.Viewers == 0)
                return;
            var la = httpClient.GetChannelInfo(Name, client_id);
            if (la.Item1 != Status.Online)
                return;
            IrcClient.SendChatMessage($"Сейчас стрим смотрит {la.Item2} человек Jebaited ", msg, ChatModeInterval);
        }

        private void CommandMusic(ChatContext db, Message msg)
        {
            var prumi = Profiller.Profiller.GetProfileOrDefault(Name);
            if (prumi == null)
                Profiller.Profiller.TryCreateProfile(Name);
            if (prumi != null && prumi.Music == 0)
                return;
            if (StatusStream != Status.Online)
                return;
            var ch = db.Channels.FirstOrDefault(a => a.Channel_id == Id);

            if (ch.VkId as object != null)
            {
                if (ch.VkId == 0)
                {
                    IrcClient.SendChatMessage("Не установлен Vk Id для канала, см. !help", msg, ChatModeInterval);
                }
                string trackname = Vkontakte.getNameTrack(ch.VkId);
                if (string.IsNullOrEmpty(trackname))
                {
                    if (ch.DjId as object != null)
                    {
                        if (ch.DjId == 0)
                        {
                            Thread.Sleep(400);
                            IrcClient.SendChatMessage("Не установлен DjId для канала, см. !help", msg, ChatModeInterval);
                            return;
                        }
                        string g = httpClient.GetMusicFromTwitchDJ(ch.DjId.ToString()).Result;
                        if (string.IsNullOrEmpty(g))
                        {
                            IrcClient.SendChatMessage("В данный момент музыка нигде не играет FeelsBadMan !", msg, ChatModeInterval);
                        }
                        else
                        {
                            IrcClient.SendChatMessage(g, msg, ChatModeInterval);
                            return;
                        }
                    }
                }
                else
                {
                    IrcClient.SendChatMessage("Сейчас в VK играет: " + trackname + " Kreygasm", msg, ChatModeInterval);
                }
            }
        }

        private void CommandDjId(ChatContext db, Message msg)
        {
            var prom = Profiller.Profiller.GetProfileOrDefault(Name);
            if (prom == null)
                Profiller.Profiller.TryCreateProfile(Name);
            if (prom != null && prom.Djid == 0)
                return;

            var pe = db.Users.FirstOrDefault(a => a.Username == msg.UserName)?.Id;
            if (pe == null)
                return;
            var mode = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == pe && a.Moderator);
            if (mode == null && msg.UserName != "dudelka_krasnaya")
                return;
            var chih = db.Channels.FirstOrDefault(a => a.Channel_name == Name);
            if (chih == null)
                return;
            int v;
            int.TryParse(msg.Djid, out v);
            if (v == 0)
                return;
            chih.DjId = v;
            ircClient.SendChatMessage("DjId сохранено!", msg, ChatModeInterval);
            db.SaveChanges();
        }

        private void CommandVkId(ChatContext db, Message msg)
        {
            var promi = Profiller.Profiller.GetProfileOrDefault(Name);
            if (promi == null)
                Profiller.Profiller.TryCreateProfile(Name);
            if (promi != null && promi.Vkid == 0)
                return;

            var t = db.Users.FirstOrDefault(a => a.Username == msg.UserName)?.Id;
            if (t == null)
                return;
            var mod = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == t && a.Moderator);
            if (mod == null)
                return;
            var mat = Regex.Match(msg.Vkid, @"id(?<id>\d+)$");
            if (msg.Vkid.Contains("id"))
            {
                mat = Regex.Match(msg.Vkid, @"id(?<id>\d+)$");

                if (mat.Success)
                {
                    var d = long.Parse(mat.Groups["id"].Value);
                    if (Vkontakte.userExist(d))
                        db.Channels.FirstOrDefault(a => a.Channel_name == Name).VkId = (int)d;
                    else return;
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
                        return;
                }
                else
                    return;
            }
            db.SaveChanges();
        }

        private void CommandCounter(ChatContext db, Message msg)
        {
            var promic = Profiller.Profiller.GetProfileOrDefault(Name);
            if (promic == null)
                Profiller.Profiller.TryCreateProfile(Name);
            if (promic != null && promic.Counter == 0)
                return;
            var userCounter = db.Users.FirstOrDefault(a => a.Username == msg.UserName);
            if (userCounter == null)
                return;
            var chCounter = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == userCounter.Id);
            if (chCounter == null)
                return;
            if (chCounter.Moderator || msg.UserName == "dudelka_krasnaya" || msg.UserName == Name)
            {
                var channel = db.Channels.FirstOrDefault(a => a.Channel_name == msg.Channel);
                if (channel == null)
                    return;
                var counters = db.Counters.Where(a => a.Channel_id == channel.Channel_id);

                if (string.IsNullOrEmpty(msg.Sign))
                {
                    if (counters == null)
                        return;
                    var j = IdReg.Match(msg.Data);
                    if (j.Success)
                    {
                        SendWhisperMessage(j.Groups["id"].Value, msg.UserName, counters.Select(a => a.Counter_name + ": " + (a.Description ?? string.Empty) + " = " + a.Count.ToString()).ToList());
                        return;
                    }
                }

                switch (msg.Sign)
                {
                    case "+":
                        if (counters != null)
                        {
                            var y = counters.FirstOrDefault(a => a.Counter_name == msg.NewName);
                            if (y == null)
                            {
                                if (!string.IsNullOrEmpty(msg.Description))
                                    db.Counters.Add(new Counters(Id, msg.NewName, msg.Description));
                                else
                                    db.Counters.Add(new Counters(Id, msg.NewName));
                                db.SaveChanges();
                            }
                            else
                            {
                                IrcClient.SendChatBroadcastMessage($"/me Cчетчик {msg.NewName} уже существует! ПодУмойте над другим названием! LUL", msg);
                                break;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(msg.Description))
                                db.Counters.Add(new Counters(Id, msg.NewName, msg.Description));
                            else
                                db.Counters.Add(new Counters(Id, msg.NewName));
                            db.SaveChanges();
                        }
                        IrcClient.SendChatBroadcastMessage($"/me Добавлен новый счетчик {msg.NewName}", msg);
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
        }

        private void CommandExistedCounter(ChatContext db, Message msg)
        {
            var promict = Profiller.Profiller.GetProfileOrDefault(Name);
            if (promict == null)
                Profiller.Profiller.TryCreateProfile(Name);
            if (promict != null && promict.Counter == 0)
                return;
            var exCounter = db.Users.FirstOrDefault(a => a.Username == msg.UserName);
            if (exCounter == null)
                return;
            var chexCounter = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == exCounter.Id);
            if (chexCounter == null)
                return;
            if (chexCounter.Moderator || msg.UserName == "dudelka_krasnaya" || msg.UserName == Name)
            {
                var channel = db.Channels.FirstOrDefault(a => a.Channel_name == msg.Channel);
                if (channel == null)
                    return;
                var counters = db.Counters.Where(a => a.Channel_id == channel.Channel_id);
                if (counters == null)
                    return;

                var y = counters.FirstOrDefault(a => a.Counter_name == msg.NewName);
                if (y == null)
                    return;
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
                        if (string.IsNullOrEmpty(y.Description))
                            IrcClient.SendChatBroadcastMessage(string.Format("/me {0}: Умерли всего {1} {2} LUL", y.Counter_name, y.Count, Helper.GetDeclension(y.Count, "раз", "раза", "раз")), Name);
                        else
                            IrcClient.SendChatBroadcastMessage(string.Format("/me {0}: {1} {2} {3} LUL", y.Counter_name, y.Description, y.Count, Helper.GetDeclension(y.Count, "раз", "раза", "раз")), Name);
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
                if (oldvalue != y.Count)
                {
                    if (string.IsNullOrEmpty(y.Description))
                        IrcClient.SendChatBroadcastMessage(string.Format("/me {0}: Умерли всего {1} {2} FeelsBadMan", y.Counter_name, y.Count, Helper.GetDeclension(y.Count, "раз", "раза", "раз")), Name);
                    else
                        IrcClient.SendChatBroadcastMessage(string.Format("/me {0}: {1} {2} {3} FeelsBadMan", y.Counter_name, y.Description, y.Count, Helper.GetDeclension(y.Count, "раз", "раза", "раз")), Name);
                }
            }
        }

        private void CommandQUpdate(ChatContext db, Message msg)
        {
            var promis = Profiller.Profiller.GetProfileOrDefault(Name);
            if (promis == null)
                Profiller.Profiller.TryCreateProfile(Name);
            if (promis != null && promis.Qupdate == 0)
                return;

            var Userqupdate = db.Users.FirstOrDefault(a => a.Username == msg.UserName);
            if ((Userqupdate != null && db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == Userqupdate.Id && a.Moderator && Userqupdate.Username == "plotoiadnuikeksik") != null) || msg.UserName == "dudelka_krasnaya")
            {
                int x = msg.QuoteNumber;
                if (x <= 0)
                    return;
                var uu = db.Quotes.Where(a => a.Channel_id == Id).ToList();

                var quo = uu.FirstOrDefault(a => a.Number == x);
                if (quo != null)
                {
                    quo.Quote = msg.Quote;
                    if (msg.Date != null)
                        quo.Date = msg.Date;
                    IrcClient.SendChatMessage(string.Format("Цитата№{0} - отредактирована", quo.Number), msg, ChatModeInterval);
                }
                else
                {

                    var o = new Quotes(Id, msg.Quote, msg.Date != null ? msg.Date : DateTime.Now, x);
                    db.Quotes.Add(o);
                    db.SaveChanges();
                    IrcClient.SendChatMessage(string.Format("Цитата№{0} : {1} - добавлена", o.Number, o.Quote), msg, ChatModeInterval);
                }
                db.SaveChanges();
            }
        }

        private void CommandQuote(ChatContext db, Message msg)
        {
            var prome = Profiller.Profiller.GetProfileOrDefault(Name);
            if (prome == null)
                Profiller.Profiller.TryCreateProfile(Name);
            if (prome != null && prome.Quote == 0)
                return;

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
                return;
            }
            bool Moder;
            if (msg.UserName == "dudelka_krasnaya")
                Moder = true;
            else
            {
                var userModer = db.Users.FirstOrDefault(a => a.Username == msg.UserName);
                if (userModer == null)
                    return;
                chusModer = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == userModer.Id);
                if (chusModer == null)
                    return;

                Moder = chusModer.Moderator && userModer.Username == "plotoiadnuikeksik";
                var l = SubReg.Match(msg.Data);
                if (l.Success)
                {
                    int b = int.Parse(l.Groups["sub"].Value);
                    if (b > chusModer.CountSubscriptions)
                        chusModer.CountSubscriptions = b;
                    db.SaveChanges();
                }
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
                            IrcClient.SendChatMessage(string.Format("\"{0}\" - {1}, {2:yyyy} цитата #{3}", r.Quote, Name, r.Date, q), msg, ChatModeInterval);
                        //ircClient.SendChatMessage(string.Format("Цитата №{0} от {1:dd/MM/yyyy} : {2}", q, r.Date, r.Quote), msg);
                    }
                    break;
            }
            db.SaveChanges();
        }
        #endregion

        #region WhisperChatCommands

        private void CommandWhisperDiscord(ChatContext db, Message msg)
        {
            var dis = Profiller.Profiller.GetProfileOrDefault(Name);
            if (dis == null)
                Profiller.Profiller.TryCreateProfile(Name);
            if (dis != null && dis.Help == 0)
                return;
            var ud = IdReg.Match(msg.Data);
            if (ud.Success)
            {
                SendWhisperMessage(ud.Groups["id"].Value, msg.UserName, "Ссылка на канал в discord: https://discordapp.com/invite/fsofbsadw");
            }
        }

        private void CommandWhisperStreamerTime(ChatContext db, Message msg)
        {
            //var prof = Profiller.Profiller.GetProfileOrDefault(Name);
            //if (prof == null)
            //    Profiller.Profiller.TryCreateProfile(Name);
            //if (prof != null && prof.Streamertime == 0)
            //    return;

            //var info = httpClient.GetWeatherInTown("ufa");
            //IrcClient.SendChatMessage("Время в Уфе - " + DateTime.Now.AddHours(2).TimeOfDay.ToString().Remove(8) + " " + (info != null ? info.Item1 + ", " + info.Item2 : " Погода сейчас недоступна :("), msg);
            IrcClient.SendChatWhisperMessage("Время у УФЕ - " + DateTime.Now.AddHours(2).TimeOfDay.ToString().Remove(8), msg);
        }

        private void CommandWhisperHelp(ChatContext db, Message msg)
        {
            var profi = Profiller.Profiller.GetProfileOrDefault(Name);
            if (profi == null)
                Profiller.Profiller.TryCreateProfile(Name);
            if (profi != null && profi.Help == 0)
                return;
            var u = IdReg.Match(msg.Data);
            if (u.Success)
            {
                //SendWhisperMessage(u.Groups["id"].Value, msg.UserName, Commands);
                SendWhisperMessage(u.Groups["id"].Value, msg.UserName, "Список комманд бота здесь: https://github.com/Xambey/DudelkaBot/wiki/Commands");
            }
        }

        private void CommandWhisperMoscowTime(ChatContext db, Message msg)
        {
            //var profil = Profiller.Profiller.GetProfileOrDefault(Name);
            //if (profil == null)
            //    Profiller.Profiller.TryCreateProfile(Name);
            //if (profil != null && profil.Moscowtime == 0)
            //    return;

            //var inf = httpClient.GetWeatherInTown("moskva");
            //IrcClient.SendChatMessage("Время в москве: " + DateTime.Now.TimeOfDay.ToString().Remove(8) + " " + (inf != null ? inf.Item1 + ", " + inf.Item2 : " Погода сейчас недоступна :("), msg);
            IrcClient.SendChatWhisperMessage("Время в москве: " + DateTime.Now.TimeOfDay.ToString().Remove(8), msg);
        }

        private void CommandWhisperMyStat(ChatContext db, Message msg)
        {
            var profile = Profiller.Profiller.GetProfileOrDefault(Name);
            if (profile == null)
                Profiller.Profiller.TryCreateProfile(Name);
            if (profile != null && profile.Mystat == 0)
                return;

            var userStat = db.Users.FirstOrDefault(a => a.Username == msg.UserName);
            if (userStat == null)
                return;
            var chusStat = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == userStat.Id);
            var math = SubReg.Match(msg.Data);
            if (math.Success)
                chusStat.CountSubscriptions = int.Parse(math.Groups["sub"].Value) > chusStat.CountSubscriptions ? int.Parse(math.Groups["sub"].Value) : chusStat.CountSubscriptions;
            db.SaveChanges();
            IrcClient.SendChatWhisperMessage("Вы написали " + chusStat.CountMessage.ToString() + " сообщений на канале" + (chusStat.CountSubscriptions > 0 ? ", также вы подписаны уже " + chusStat.CountSubscriptions.ToString() + " месяца(ев) MrDestructoid  " : ""), msg);
        }

        private void CommandWhisperSexyLevel(ChatContext db, Message msg)
        {
            //var prus = Profiller.Profiller.GetProfileOrDefault(Name);
            //if (prus == null)
            //    Profiller.Profiller.TryCreateProfile(Name);
            //if (prus != null && prus.Sexylevel == 0)
            //    return;
            var m = SubReg.Match(msg.Data);
            if (m.Success)
            {
                var usLevel = db.Users.FirstOrDefault(a => a.Username == msg.UserName);
                if (usLevel == null)
                    return;
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
        }

        private void CommandWhisperDjId(ChatContext db, Message msg)
        {
            var prom = Profiller.Profiller.GetProfileOrDefault(Name);
            if (prom == null)
                Profiller.Profiller.TryCreateProfile(Name);
            if (prom != null && prom.Djid == 0)
                return;

            var pe = db.Users.FirstOrDefault(a => a.Username == msg.UserName)?.Id;
            if (pe == null)
                return;
            var mode = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == pe && a.Moderator);
            if (mode == null && msg.UserName != "dudelka_krasnaya")
                return;
            var chih = db.Channels.FirstOrDefault(a => a.Channel_name == Name);
            if (chih == null)
                return;
            int v;
            int.TryParse(msg.Djid, out v);
            if (v == 0)
                return;
            chih.DjId = v;
            ircClient.SendChatWhisperMessage("DjId сохранено!", msg);
            db.SaveChanges();
        }

        private void CommandWhisperVkId(ChatContext db, Message msg)
        {
            var promi = Profiller.Profiller.GetProfileOrDefault(Name);
            if (promi == null)
                Profiller.Profiller.TryCreateProfile(Name);
            if (promi != null && promi.Vkid == 0)
                return;

            var t = db.Users.FirstOrDefault(a => a.Username == msg.UserName)?.Id;
            if (t == null)
                return;
            var mod = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == t && a.Moderator);
            if (mod == null)
                return;
            var mat = Regex.Match(msg.Vkid, @"id(?<id>\d+)$");
            if (msg.Vkid.Contains("id"))
            {
                mat = Regex.Match(msg.Vkid, @"id(?<id>\d+)$");

                if (mat.Success)
                {
                    var d = long.Parse(mat.Groups["id"].Value);
                    if (Vkontakte.userExist(d))
                        db.Channels.FirstOrDefault(a => a.Channel_name == Name).VkId = (int)d;
                    else return;
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
                        return;
                }
                else
                    return;
            }
            db.SaveChanges();
        }

        private void CommandWhisperCounter(ChatContext db, Message msg)
        {
            var promic = Profiller.Profiller.GetProfileOrDefault(Name);
            if (promic == null)
                Profiller.Profiller.TryCreateProfile(Name);
            if (promic != null && promic.Counter == 0)
                return;
            var userCounter = db.Users.FirstOrDefault(a => a.Username == msg.UserName);
            if (userCounter == null)
                return;
            var chCounter = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == userCounter.Id);
            if (chCounter == null)
                return;
            if (chCounter.Moderator || msg.UserName == "dudelka_krasnaya" || msg.UserName == Name)
            {
                var channel = db.Channels.FirstOrDefault(a => a.Channel_name == msg.Channel);
                if (channel == null)
                    return;
                var counters = db.Counters.Where(a => a.Channel_id == channel.Channel_id);

                if (string.IsNullOrEmpty(msg.Sign))
                {
                    if (counters == null)
                        return;
                    var j = IdReg.Match(msg.Data);
                    if (j.Success)
                    {
                        SendWhisperMessage(j.Groups["id"].Value, msg.UserName, counters.Select(a => a.Counter_name + ": " + (a.Description ?? string.Empty) + " = " + a.Count.ToString()).ToList());
                        return;
                    }
                }

                switch (msg.Sign)
                {
                    case "+":
                        if (counters != null)
                        {
                            var y = counters.FirstOrDefault(a => a.Counter_name == msg.NewName);
                            if (y == null)
                            {
                                if (!string.IsNullOrEmpty(msg.Description))
                                    db.Counters.Add(new Counters(Id, msg.NewName, msg.Description));
                                else
                                    db.Counters.Add(new Counters(Id, msg.NewName));
                                db.SaveChanges();
                            }
                            else
                            {
                                IrcClient.SendChatWhisperMessage($"/me Cчетчик {msg.NewName} уже существует! ПодУмойте над другим названием! LUL", msg);
                                break;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(msg.Description))
                                db.Counters.Add(new Counters(Id, msg.NewName, msg.Description));
                            else
                                db.Counters.Add(new Counters(Id, msg.NewName));
                            db.SaveChanges();
                        }
                        IrcClient.SendChatWhisperMessage($"/me Добавлен новый счетчик {msg.NewName}", msg);
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
        }

        private void CommandWhisperExistedCounter(ChatContext db, Message msg)
        {
            var promict = Profiller.Profiller.GetProfileOrDefault(Name);
            if (promict == null)
                Profiller.Profiller.TryCreateProfile(Name);
            if (promict != null && promict.Counter == 0)
                return;
            var exCounter = db.Users.FirstOrDefault(a => a.Username == msg.UserName);
            if (exCounter == null)
                return;
            var chexCounter = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == exCounter.Id);
            if (chexCounter == null)
                return;
            if (chexCounter.Moderator || msg.UserName == "dudelka_krasnaya" || msg.UserName == Name)
            {
                var channel = db.Channels.FirstOrDefault(a => a.Channel_name == msg.Channel);
                if (channel == null)
                    return;
                var counters = db.Counters.Where(a => a.Channel_id == channel.Channel_id);
                if (counters == null)
                    return;

                var y = counters.FirstOrDefault(a => a.Counter_name == msg.NewName);
                if (y == null)
                    return;
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
                        if (string.IsNullOrEmpty(y.Description))
                            IrcClient.SendChatBroadcastMessage(string.Format("/me {0}: Умерли всего {1} {2} LUL", y.Counter_name, y.Count, Helper.GetDeclension(y.Count, "раз", "раза", "раз")), Name);
                        else
                            IrcClient.SendChatBroadcastMessage(string.Format("/me {0}: {1} {2} {3} LUL", y.Counter_name, y.Description, y.Count, Helper.GetDeclension(y.Count, "раз", "раза", "раз")), Name);
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
                if (oldvalue != y.Count)
                {
                    if (string.IsNullOrEmpty(y.Description))
                        IrcClient.SendChatBroadcastMessage(string.Format("/me {0}: Умерли всего {1} {2} FeelsBadMan", y.Counter_name, y.Count, Helper.GetDeclension(y.Count, "раз", "раза", "раз")), Name);
                    else
                        IrcClient.SendChatBroadcastMessage(string.Format("/me {0}: {1} {2} {3} FeelsBadMan", y.Counter_name, y.Description, y.Count, Helper.GetDeclension(y.Count, "раз", "раза", "раз")), Name);
                }
            }
        }

        private void CommandWhisperQUpdate(ChatContext db, Message msg)
        {
            var promis = Profiller.Profiller.GetProfileOrDefault(Name);
            if (promis == null)
                Profiller.Profiller.TryCreateProfile(Name);
            if (promis != null && promis.Qupdate == 0)
                return;

            var Userqupdate = db.Users.FirstOrDefault(a => a.Username == msg.UserName);
            if ((Userqupdate != null && db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == Userqupdate.Id && a.Moderator && Userqupdate.Username == "plotoiadnuikeksik") != null) || msg.UserName == "dudelka_krasnaya")
            {
                int x = msg.QuoteNumber;
                if (x <= 0)
                    return;
                var uu = db.Quotes.Where(a => a.Channel_id == Id).ToList();

                var quo = uu.FirstOrDefault(a => a.Number == x);
                if (quo != null)
                {
                    quo.Quote = msg.Quote;
                    if (msg.Date != null)
                        quo.Date = msg.Date;
                    IrcClient.SendChatMessage(string.Format("Цитата№{0} - отредактирована", quo.Number), msg);
                }
                else
                {

                    var o = new Quotes(Id, msg.Quote, msg.Date != null ? msg.Date : DateTime.Now, x);
                    db.Quotes.Add(o);
                    db.SaveChanges();
                    IrcClient.SendChatMessage(string.Format("Цитата№{0} : {1} - добавлена", o.Number, o.Quote), msg);
                }
                db.SaveChanges();
            }
        }

        private void CommandWhisperQuote(ChatContext db, Message msg)
        {
            var prome = Profiller.Profiller.GetProfileOrDefault(Name);
            if (prome == null)
                Profiller.Profiller.TryCreateProfile(Name);
            if (prome != null && prome.Quote == 0)
                return;

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
                return;
            }
            bool Moder;
            if (msg.UserName == "dudelka_krasnaya")
                Moder = true;
            else
            {
                var userModer = db.Users.FirstOrDefault(a => a.Username == msg.UserName);
                if (userModer == null)
                    return;
                chusModer = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == userModer.Id);
                if (chusModer == null)
                    return;

                Moder = chusModer.Moderator && userModer.Username == "plotoiadnuikeksik";
                var l = SubReg.Match(msg.Data);
                if (l.Success)
                {
                    int b = int.Parse(l.Groups["sub"].Value);
                    if (b > chusModer.CountSubscriptions)
                        chusModer.CountSubscriptions = b;
                    db.SaveChanges();
                }
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
        }
        #endregion

        #region Handlers

        private bool isUserVoted(Message msg)
        {
            foreach (var item in VoteResult)
            {
                if (item.Value.FirstOrDefault(a => a.UserName == msg.UserName) != null)
                {
                    return true;
                }
            }
            return false;
        }

        private void HandlerNoticeMessage(Message msg)
        {
            ChatMode = msg.ChatMode;
            switch (msg.ChatMode)
            {
                case ChatMode.slow_off:
                    ChatModeInterval = TimeSpan.Zero;
                    break;
                case ChatMode.slow_on:
                    ChatModeInterval = TimeSpan.FromSeconds(double.Parse(string.Concat(msg.Msg.Where(a => char.IsDigit(a)))));
                    break;
                case ChatMode.subs_on:
                    ChatModeInterval = TimeSpan.MaxValue;
                    break;
                case ChatMode.subs_off:
                    ChatModeInterval = TimeSpan.Zero;
                    break;
                case ChatMode.msg_subsonly:
                    ChatModeInterval = TimeSpan.FromSeconds(double.Parse(string.Concat(msg.Msg.Where(a => char.IsDigit(a)))));
                    break;
                default:
                    ChatModeInterval = TimeSpan.Zero;
                    break;
            }
        }

        private void VoteHandlerMessage(Message msg)
        {
            if ((VoteResult.ContainsKey(msg.Msg) || (msg.Msg.All(char.IsDigit) && int.Parse(msg.Msg) <= VoteResult.Count ? true : false)) && !isUserVoted(msg))
            {
                if (msg.Msg.All(char.IsDigit) && !VoteResult.ContainsKey(msg.Msg))
                {
                    VoteResult[VoteResult.ElementAt(int.Parse(msg.Msg) - 1).Key].Add(new User(msg.UserName));
                }
                else
                    VoteResult[msg.Msg].Add(new User(msg.UserName));
            }
        }

        private void SubscribeMessage(Message msg, ChatContext db)
        {
            Logger.WriteLineMessage(msg.Data);
            if (msg.Data.Contains("while\\syou\\swere\\saway") || msg.Data.Contains("while you were away"))
                return;
            var userNotify = db.Users.FirstOrDefault(a => a.Username == msg.SubscriberName);
            int chussub = 1;
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
                    chussub = chus.CountSubscriptions;
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
                    chus.Active = true;
                    if (msg.Subscription > chus.CountSubscriptions)
                        chus.CountSubscriptions = msg.Subscription;
                    else
                        chus.CountSubscriptions++;
                    chussub = chus.CountSubscriptions;
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

            ircClient.SendChatBroadcastMessage(p.GetRandomSubAnswer().Replace("month", chussub.ToString()).Replace("nick", msg.SubscriberName), Name);
            //IrcClient.SendChatMessage(string.Format("Спасибо за подписку! Добро пожаловать к нам, с {0} - месяцем тебя Kappa {1}", chussub > msg.Subscription ? chussub : msg.Subscription, chussub > msg.Subscription ? "Псс. Я тебя помню, меня не обманешь Kappa , добро пожаловать снова! ":""), msg.SubscriberName, msg);
        }

        private void UsernoticeMessage(Message msg, ChatContext db)
        {
            //Logger.WriteLineMessage(msg.Data);
            if (subscribedList.Contains(msg.UserName))
                return;
            bool isSub;
            switch (msg.Msg_id)
            {
                case "resub":
                    isSub = false;
                    break;
                case "sub":
                    isSub = true;
                    break;
                default:
                    return;
            }

            var prof = Profiller.Profiller.GetProfileOrDefault(msg.Channel);
            if (prof == null || isSub && prof.Activities.Subscriptions == 0 || !isSub && prof.Activities.Resubscriptions == 0)
                return;

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
            if (j != null)
            {
                var p = Profiller.Profiller.GetProfileOrDefault(Name);
                if (p == null)
                {
                    Profiller.Profiller.TryCreateProfile(Name);
                    p = Profiller.Profiller.GetProfileOrDefault(Name);
                }
                subscribedList.Add(msg.SubscriberName);
                if (isSub)
                    ircClient.SendChatBroadcastMessage(p.GetRandomSubAnswer().Replace("month", j.CountSubscriptions.ToString()).Replace("nick", msg.SubscriberName), Name);
                else // id != charity
                    ircClient.SendChatBroadcastMessage(p.GetRandomResubAnswer().Replace("month", j.CountSubscriptions.ToString()).Replace("nick", msg.SubscriberName), Name);
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
                            HandlerNoticeMessage(msg);
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
                        case TypeMessage.WHISPER:
                            switch (msg.Command)
                            {
                                case Command.discord:
                                    CommandWhisperDiscord(db, msg);
                                    break;
                                case Command.streamertime:
                                    CommandWhisperStreamerTime(db, msg);
                                    break;
                                case Command.help:
                                    CommandWhisperHelp(db, msg);
                                    break;
                                case Command.moscowtime:
                                    CommandWhisperMoscowTime(db, msg);
                                    break;
                                case Command.mystat:
                                    CommandWhisperMyStat(db, msg);
                                    break;
                                case Command.sexylevel:
                                    CommandWhisperSexyLevel(db, msg);
                                    break;
                                case Command.unknown:
                                    //lock (ErrorListMessages)
                                    //    ErrorListMessages.Add(msg);
                                    break;
                                case Command.djid:
                                    CommandWhisperDjId(db, msg);
                                    break;
                                case Command.vkid:
                                    CommandWhisperVkId(db, msg);
                                    break;
                                case Command.counter:
                                    CommandWhisperCounter(db, msg);
                                    break;
                                case Command.existedcounter:
                                    CommandWhisperExistedCounter(db, msg);
                                    break;
                                case Command.qupdate:
                                    CommandWhisperQUpdate(db, msg);
                                    break;
                                case Command.quote:
                                    CommandWhisperQuote(db, msg);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case TypeMessage.PRIVMSG:
                            HandlerCountMessages(msg, db);

                            if (VoteActive)
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
                                //else
                                //{
                                //    math = AnswerReg.Match(msg.Msg);
                                //    if (math.Success)
                                //    {
                                //        IrcClient.SendChatMessage(math.Groups["text"].Value, msg);
                                //        break;
                                //    }
                                //}
                            }

                            switch (msg.Command)
                            {
                                case Command.wakeup:
                                    CommandWakeUp(db, msg);
                                    break;
                                case Command.sleep:
                                    CommandSleep(db, msg);
                                    break;
                                case Command.discord:
                                    CommandDiscord(db, msg);
                                    break;
                                case Command.reconnect:
                                    CommandReconnect(db, msg);
                                    break;
                                case Command.ball:
                                    CommandBall(db, msg);
                                    break;
                                case Command.voteLite:
                                    CommandVoteLite(db, msg);
                                    break;
                                case Command.vote:
                                    CommandVote(db, msg);
                                    break;
                                case Command.advert:
                                    CommandAdvert(db, msg);
                                    break;
                                case Command.streamertime:
                                    CommandStreamerTime(db, msg);
                                    break;
                                case Command.help:
                                    CommandHelp(db, msg);
                                    break;
                                case Command.moscowtime:
                                    CommandMoscowTime(db, msg);
                                    break;
                                case Command.mystat:
                                    CommandMyStat(db, msg);
                                    break;
                                case Command.toplist:
                                    CommandTopList(db, msg);
                                    break;
                                case Command.uptime:
                                    CommandUpTime(db, msg);
                                    break;
                                case Command.sexylevel:
                                    //CommandSexyLevel(db, msg);
                                    break;
                                case Command.members:
                                    CommandMembers(db, msg);
                                    break;
                                case Command.unknown:
                                    //lock (ErrorListMessages)
                                    //    ErrorListMessages.Add(msg);
                                    break;
                                case Command.viewers:
                                    CommandViewers(db, msg);
                                    break;
                                case Command.music:
                                    CommandMusic(db, msg);
                                    break;
                                case Command.djid:
                                    CommandDjId(db, msg);
                                    break;
                                case Command.vkid:
                                    CommandVkId(db, msg);
                                    break;
                                case Command.counter:
                                    CommandCounter(db, msg);
                                    break;
                                case Command.existedcounter:
                                    CommandExistedCounter(db, msg);
                                    break;
                                case Command.qupdate:
                                    CommandQUpdate(db, msg);
                                    break;
                                case Command.quote:
                                    CommandQuote(db, msg);
                                    break;
                                default:
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
                if (ex.InnerException != null)
                    Logger.ShowLineCommonMessage(ex.InnerException.Source + " " + ex.InnerException.Message + " " + ex.InnerException.Data + " " + ex.InnerException.StackTrace);
                Console.ForegroundColor = ConsoleColor.Gray;
                return;
            }
        }

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
                        //FindDublicateUsers(db);
                        foreach (var item in db.ChannelsUsers.Where(a => a.Channel_id == Id))
                        {
                            item.Active = false;
                        }
                    }
                    db.SaveChanges();

                    //findDublicateUsers(db);
                }
                if (Profiller.Profiller.GetProfileOrDefault(Name) == null)
                    Profiller.Profiller.TryCreateProfile(Name);

                httpClient.GetChannelInfo(Name, client_id);
                Logger.UpdateChannelPaths(Name);

                StreamChatTimer = new Timer(StreamStateChatUpdate, null, StreamStateChatUpdateTime * 60000, StreamStateChatUpdateTime * 60000);
                StreamTimer = new Timer(StreamStateUpdate, null, StreamStateUpdateTime * 60000, StreamStateUpdateTime * 60000);
                var prof = Profiller.Profiller.GetProfileOrDefault(Name).Activities;

                if (prof.ShowQuote == 1)
                {
                    QuoteTimer = new Timer(ShowRandowQuote, null, QuoteShowTime * 60000, QuoteShowTime * 60000);
                }
                if (prof.CheckSubscriptions == 1)
                {
                    OAuth = prof.Oauth;
                    Client_ID = prof.Client_ID;
                    do
                    {
                        NetId = httpClient.GetChannelId(Name, client_id).Item1;
                        Thread.Sleep(5000);
                    }
                    while (NetId == null);
                    CheckSubscriptions = new Timer(CheckStateSubscriptions, null, CheckStateSubscriptionsTime, CheckStateSubscriptionsTime);
                }
                if (prof.ShowChangedMusic == 1)
                {
                    ShowChangedMusicTimer = new Timer(CheckStateMusicStatus, null, CheckMusicChangedTime, CheckMusicChangedTime);
                    using (var db = new ChatContext())
                    {
                        var ch = db.Channels.FirstOrDefault(a => a.Channel_name == Name);
                        string b = Vkontakte.getNameTrack(ch.VkId);
                        if (string.IsNullOrEmpty(b))
                        {
                            b = httpClient.GetMusicFromTwitchDJ(ch.DjId.ToString()).Result;
                            if (!string.IsNullOrEmpty(b))
                                LastMusic = b;
                        }
                        else
                            LastMusic = b;
                    }
                }
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
            try
            {
                if (IrcClient != null && DateTime.Now - IrcClient.LastPingResponseMessageTime > TimeSpan.FromMinutes(10))
                {
                    Reconnect();
                    IrcClient.LastPingResponseMessageTime = DateTime.Now;
                    return;
                }
                if (IrcClient != null && ircClient.isDisconnect())
                    throw new Exception();
                else if (ircClient == null)
                    ircClient = new IrcClient(iphost, port, userName, password);
                IrcClient.isConnect();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Logger.ShowLineCommonMessage(ex.StackTrace + ex.Data + ex.Message);
                if (ex.InnerException != null)
                    Logger.ShowLineCommonMessage(ex.InnerException.StackTrace + ex.InnerException.Data + ex.InnerException.Message);
                Console.ForegroundColor = ConsoleColor.Gray;
                IrcClient.isConnect();
                return;
            }
        }

        private static void FindDublicateUsers(ChatContext db)
        {
            Dictionary<string, int> map = new Dictionary<string, int>();
            Dictionary<string, int> map2 = new Dictionary<string, int>();
            foreach (var item in db.Users)
            {
                if (string.IsNullOrEmpty(item.Username))
                    item.Username = "void";
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
                    try
                    {
                        var us = db.Users.FirstOrDefault(a => a.Username == item.Key);
                        while (us != null)
                        {
                            db.Users.Remove(us);
                            us = db.Users.FirstOrDefault(a => a.Username == item.Key);
                            db.SaveChanges();
                        }
                        Console.Write("User " + item.Key + " double: " + item.Value.ToString());
                        foreach (var g in db.Users)
                        {
                            if (g.Username == item.Key)
                            {
                                Console.Write(" " + g.Id.ToString());
                            }
                        }
                        Console.WriteLine();
                    }
                    catch {
                        continue;
                    }
                }
            }
            db.SaveChanges();
        }

        private void ShowRandowQuote(object obj)
        {
            if (StatusStream == Status.Online && countMessageQuote > countLimitMessagesForShowQuote)
            {
                using (var db = new ChatContext()) {

                    var or = db.Quotes.Where(a => a.Channel_id == Id).ToList();

                    if (or.Count == NumbersOfQuotations.Count)
                        NumbersOfQuotations.Clear();
                    if (or.Count <= 0)
                    {
                        countMessageQuote = 0;
                        return;
                    }
                    int c = Rand.Next(1, or.Count());
                    while (NumbersOfQuotations.Contains(c))
                        c = Rand.Next(1, or.Count());
                    NumbersOfQuotations.Add(c);
                    var quot = or.SingleOrDefault(a => a.Number == c);
                    if (quot != null)
                        IrcClient.SendChatBroadcastMessage(string.Format("/me Случайная цитата LUL - {0}, {1:yyyy} #{2} : '{3}'", Name, quot.Date, c, quot.Quote), Name);
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
                IrcClient.Reconnect();
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

        private void CheckStateSubscriptions(object obj)
        {
            try
            {
                //if (StatusStream != Status.Online)
                //    return;
                var net = httpClient.GetChannelSubscribers(NetId, Client_ID, OAuth);
                if (net != null && !lastNetSubscribers.LastOrDefault().Equals(net.LastOrDefault())) {
                    lastNetSubscribers = net;
                    using (var db = new ChatContext())
                    {
                        foreach (var item in net)
                        {
                            if (subscribedList.Contains(item.Key))
                                continue;
                            var user = db.Users.FirstOrDefault(a => a.Username == item.Key);
                            if (user == null)
                            {
                                user = new Users(item.Key);
                                db.Users.Add(user);
                                db.SaveChanges();

                                var chus = db.ChannelsUsers.Where(a => a.Channel_id == Id).FirstOrDefault(a => a.User_id == user.Id);
                                if (chus == null)
                                {
                                    chus = new ChannelsUsers(user.Id, Id, item.Value + 1);
                                    db.ChannelsUsers.Add(chus);
                                    db.SaveChanges();

                                    var prof = Profiller.Profiller.GetProfileOrDefault(Name);
                                    if (prof == null)
                                    {
                                        Profiller.Profiller.TryCreateProfile(Name);
                                        return;
                                    }

                                    else if (item.Value <= 0)
                                        ircClient.SendChatBroadcastMessage(prof.GetRandomSubAnswer().Replace("month", (item.Value + 1).ToString()).Replace("nick", item.Key), Name);
                                    else
                                        ircClient.SendChatBroadcastMessage(prof.GetRandomResubAnswer().Replace("month", (item.Value + 1).ToString()).Replace("nick", item.Key), Name);
                                }
                                else
                                {
                                    var prof = Profiller.Profiller.GetProfileOrDefault(Name);
                                    if (prof == null)
                                    {
                                        Profiller.Profiller.TryCreateProfile(Name);
                                        return;
                                    }
                                    else if (item.Value <= 0)
                                        ircClient.SendChatBroadcastMessage(prof.GetRandomSubAnswer().Replace("month", (item.Value + 1).ToString()).Replace("nick", item.Key), Name);
                                    else
                                        ircClient.SendChatBroadcastMessage(prof.GetRandomResubAnswer().Replace("month", (item.Value + 1).ToString()).Replace("nick", item.Key), Name);
                                    chus.CountSubscriptions++;
                                    db.SaveChanges();
                                }
                            }
                            else
                            {
                                var chus = db.ChannelsUsers.FirstOrDefault(a => a.User_id == user.Id);
                                if (chus == null)
                                {
                                    chus = new ChannelsUsers(user.Id, Id, item.Value + 1);
                                    db.ChannelsUsers.Add(chus);
                                    db.SaveChanges();

                                    var prof = Profiller.Profiller.GetProfileOrDefault(Name);
                                    if (prof == null)
                                    {
                                        Profiller.Profiller.TryCreateProfile(Name);
                                        return;
                                    }
                                    subscribedList.Add(item.Key);
                                    if (item.Value <= 0)
                                        ircClient.SendChatBroadcastMessage(prof.GetRandomSubAnswer().Replace("month", (item.Value + 1).ToString()).Replace("nick", item.Key), Name);
                                    else
                                        ircClient.SendChatBroadcastMessage(prof.GetRandomResubAnswer().Replace("month", (item.Value + 1).ToString()).Replace("nick", item.Key), Name);
                                }
                                else
                                {
                                    var prof = Profiller.Profiller.GetProfileOrDefault(Name);
                                    if (prof == null)
                                    {
                                        Profiller.Profiller.TryCreateProfile(Name);
                                        return;
                                    }
                                    if (item.Value <= 0)
                                        ircClient.SendChatBroadcastMessage(prof.GetRandomSubAnswer().Replace("month", (item.Value + 1).ToString()).Replace("nick", item.Key), Name);
                                    else
                                        ircClient.SendChatBroadcastMessage(prof.GetRandomResubAnswer().Replace("month", (item.Value + 1).ToString()).Replace("nick", item.Key), Name);
                                    chus.CountSubscriptions++;
                                    subscribedList.Add(item.Key);
                                    db.SaveChanges();
                                }
                            }
                        }

                    }
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

        private void CheckStateMusicStatus(object obj)
        {
            try
            {
                var prumi = Profiller.Profiller.GetProfileOrDefault(Name);
                if (prumi == null)
                    Profiller.Profiller.TryCreateProfile(Name);
                if (prumi != null && prumi.Activities.ShowChangedMusic == 0)
                    return;
                if (StatusStream != Status.Online)
                    return;
                using (var db = new ChatContext())
                {
                    var ch = db.Channels.FirstOrDefault(a => a.Channel_id == Id);

                    string trackname = Vkontakte.getNameTrack(ch.VkId);
                    if (trackname == LastMusic)
                        return;
                    if (string.IsNullOrEmpty(trackname))
                    {
                        if (ch.DjId as object != null)
                        {
                            if (ch.DjId == 0)
                            {
                                return;
                            }
                            string g = httpClient.GetMusicFromTwitchDJ(ch.DjId.ToString()).Result;
                            httpClient.GetMusicLinkFromTwitchDJ("dariya_willis");
                            if (string.IsNullOrEmpty(g) || g == LastMusic)
                            {
                                return;
                            }
                            else
                            {
                                LastMusic = g;
                                IrcClient.SendChatBroadcastMessage("/me " + g, ch.Channel_name);
                            }
                        }
                    }
                    else
                    {
                        LastMusic = trackname;
                        IrcClient.SendChatBroadcastMessage("/me Сейчас в VK играет: " + trackname + " Kreygasm", ch.Channel_name);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Logger.ShowLineCommonMessage(ex.Source + ex.Data + ex.Message);
                if (ex.InnerException != null)
                    Logger.ShowLineCommonMessage(ex.InnerException.Source + ex.InnerException.Data + ex.InnerException.Message);
                Console.ForegroundColor = ConsoleColor.Gray;
                return;
            }
        }

        private void StreamStateUpdate(object obj)
        {
            try
            {
                var oldstatus = StatusStream;
                var info = httpClient.GetChannelInfo(Name, client_id);
                switch (info.Item1)
                {
                    case Status.Online:
                        StatusStream = Status.Online;
                        if (NumbersOfQuotations.Count != 0)
                            NumbersOfQuotations.Clear();
                        if (subscribedList.Count != 0)
                        {
                            var Bag = new System.Collections.Concurrent.ConcurrentBag<string>();
                            Interlocked.Exchange<ConcurrentBag<string>>(ref subscribedList, Bag);
                        }
                        //using (var db = new ChatContext())
                        //{
                        //    foreach (var item in db.ChannelsUsers.Where(a => a.Channel_id == Id))
                        //        item.Active = false;
                        //    db.SaveChanges();
                        //}
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
                    //if (StatusStream == Status.Online && oldstatus == Status.Offline)
                    //{
                    //    var inf = httpClient.GetWeatherInTown("ufa");
                    //    IrcClient.SendChatBroadcastMessage($"Добро пожаловать на борт! Статус стрима On Air, полет нормальный. Температура за бортом {inf.Item1}, на небе {inf.Item2}. Просим всех прибывших занять свои места и поздороваться с чатиком <3 <3 <3 . Вкусняшки будут раздаваться через некоторое время gachiGASM nymnCorn . Спасибо что сели в наш чат DuckerZ /", Name);
                    //}

                    if (StatusStream == Status.Offline || StatusStream == Status.Online)
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
                if (ex.InnerException != null)
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
                IrcClient.StartProcess();
            }
            IrcClient.JoinRoom(Name);
            Logger.ShowLineCommonMessage($"Выполнен вход в комнату: {Name} ...");
        }

        public void LeaveRoom()
        {
            if (IrcClient == null)
            {
                IrcClient = new IrcClient(Iphost, Port, UserName, Password);
                IrcClient.StartProcess();
            }
            IrcClient.LeaveRoom(Name);
            Logger.ShowLineCommonMessage($"Выполнен выход из комнаты: {Name} ...");
        }

        private static void SwitchMessage(string data)
        {
            try
            {
                Message currentMessage = new Message(data);
                if (ViewChannel != null && currentMessage.Channel == ViewChannel.Name && currentMessage.Msg != null)
                    Logger.ShowLineChannelMessage(currentMessage.UserName, currentMessage.Msg, currentMessage.Channel);
                else if (!currentMessage.Success)
                {
                    Console.ForegroundColor = ConsoleColor.Green;

                    if (ActiveLog)
                        Logger.ShowLineCommonMessage(data);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    return;
                }
                else if (currentMessage.UserName == "moobot" || currentMessage.UserName == "nightbot")
                    return;
                else if (ViewChannel != null && currentMessage.Channel != ViewChannel.Name && currentMessage.Msg != null)
                {
                    Logger.WriteLineMessage(currentMessage.UserName, currentMessage.Msg, currentMessage.Channel);
                    Logger.WriteLineMessage(currentMessage.Data);
                }

                if (currentMessage.Channel != null && Channels.ContainsKey(currentMessage.Channel))
                {
                    Channels[currentMessage.Channel].Handler(currentMessage);
                }
                else
                {
                    Channels.First().Value?.Handler(currentMessage);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Logger.ShowLineCommonMessage(ex.Source + " " + ex.Message + " " + ex.Data);
                Console.ForegroundColor = ConsoleColor.Gray;
                SmtpClient.SendEmailAsync("dudelkabot@mail.ru", "I'm not fine master", ex.Message + " " + ex.Source + " " + ex.Data + " 733 str on Channel.cs");
                return;
            }
        }

        public static void SendWhisperMessage(string touser_id, string username, List<string> message)
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

        public static void SendWhisperMessage(string touser_id, string username, string message)
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


        public static void Process()
        {
            while (true)
            {
                try
                {
                    if (IrcClient.TokenProcess != null && IrcClient.TokenProcess.IsCancellationRequested)
                    {
                        return;
                    }
                    if (ircClient == null)
                        ircClient = new IrcClient(Iphost, Port, UserName, Password);

                    ircClient.isConnect();
                    string s = ircClient.ReadMessageAsync();

                    //if (s == null)
                    //    return;
                    if (!string.IsNullOrEmpty(s))
                    {
                        Task.Run(() => SwitchMessage(s));
                    }

                    Thread.Sleep(5);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Logger.ShowLineCommonMessage(ex.Data + " " + ex.Message + " " + ex.StackTrace);
                    if (ex.InnerException != null)
                        Logger.ShowLineCommonMessage(ex.InnerException.Data + ex.InnerException.Message + ex.InnerException.StackTrace);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    SmtpClient.SendEmailAsync("dudelkabot@mail.ru", "I'm not fine master", ex.Message + " " + ex.Source + " " + ex.Data + " 1928 str on Channel.cs");
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
