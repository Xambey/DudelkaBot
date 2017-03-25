using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DudelkaBot.ircClient
{
    public class Message
    {
        #region Patterns
        public static string patternPVMSG = @":(?<username>\w+)!\w+@\w+.tmi.twitch.tv (?<type>\w+) #(?<channel>\w+) :(?<msg>.*)";
        public static string patternPRIVMSGtag = @"@.* :(?<username>\w+)!.* #(?<channel>\w+) :(?<msg>.*)";
        public static string patternPARTorJOIN = @":(?<username>\w+)!.+ (?<type>\w+) #(?<channel>\w+)";
        public static string typePattern = @" (?<type>[A-Z]+) #";
        public static string commandPattern = @"#(?<channel>\w+) :!(?<command>\w+)$";
        public static string pingPattern = @"PING\s+";
        public static string namesPattern = @":\S+ \d+ \S+\w+ = #(?<channel>\w+) :(?<users>.*)";
        public static string modePattern = @":.+ #(?<channel>\w+) (?<sign>.)o (?<username>\w+)";
        public static string usernoticePattern = @".+login=(?<username>\w+).+msg-param-months=(?<sub>\d+).* USERNOTICE #(?<channel>\w+)";
        public static string subscribePattern = @"(?<username>\w+).+";
        public static string votePattern = @"!vote (?<theme>.+):(?<time>\d+):(?<variants>.+)";
        public static string advertPattern = @"!advert (?<time>\d+) (?<count>\d+) (?<advert>.+)";
        public static string deathPattern = @"!death (?<command>v|[+-]|\d+)$";
        public static string deathBattlePattern = @"!deathbattle (?<command>v|[+-]|\d+)$";
        public static string vkidPattern = @"!vkid (?<id>\w+)$";
        public static string quotePattern = @"!quote\s+(?<op>[+-])\s+(?<some>.+)";
        public static string quoteDatePapptern = @"!quote\s+(?<op>[+-])\s+(?<date>\d{1,2}\.\d{1,2}\.\d{4})\s+(?<some>.+)";
        public static string quoteShowPattern = @"!quote (?<number>\d+)$";
        public static string quoteUpdatePattern = @"!qupdate (?<number>\d+) (?<quote>.+)";
        public static string quoteUpdateWithDate = @"!qupdate\s+(?<number>\d+)\s+(?<date>\d{1,2}\.\d{1,2}\.\d{4})\s+(?<quote>.+)"; 
        #endregion

        #region Regexes
        public static Regex typeReg = new Regex(typePattern);
        public static Regex joinOrpartReg = new Regex(patternPARTorJOIN);
        public static Regex pvmsgTagReg = new Regex(patternPRIVMSGtag);
        public static Regex pvmsgReg = new Regex(patternPVMSG);
        public static Regex commandReg = new Regex(commandPattern);
        public static Regex pingReg = new Regex(pingPattern);
        public static Regex namesReg = new Regex(namesPattern);
        public static Regex modeReg = new Regex(modePattern);
        public static Regex usernoticeReg = new Regex(usernoticePattern);
        public static Regex subscribeReg = new Regex(subscribePattern);
        public static Regex voteReg = new Regex(votePattern);
        public static Regex advertReg = new Regex(advertPattern);
        public static Regex deathReg = new Regex(deathPattern);
        public static Regex deathBattleReg = new Regex(deathBattlePattern);
        public static Regex vkidReg = new Regex(vkidPattern);
        public static Regex quoteReg = new Regex(quotePattern);
        public static Regex quoteDateReg = new Regex(quoteDatePapptern);
        public static Regex quoteShowReg = new Regex(quoteShowPattern);
        public static Regex quoteUpdateReg = new Regex(quoteUpdatePattern);
        public static Regex quoteUpdateWithDateReg = new Regex(quoteUpdateWithDate);
        #endregion

        #region Fields
        public TypeMessage Type = TypeMessage.UNKNOWN;
        public Command Command = Command.unknown;
        public int Time = 0;

        private string data;
        private string userName;
        private string subscriberName;
        private string host;
        private string msg;
        private bool success = true;
        private bool ping = false;
        private string channel;
        private List<string> namesUsers;
        private string sign;
        private int subscription = 0;
        private bool voteActive = false;
        private string theme;
        private List<string> variants;
        private int advertTime = 0;
        private int advertCount = 0;
        private string advert;
        private string deathCommand;
        private string vkid;
        private int quoteNumber = 0;
        private string quote;
        private string quoteOperation;
        private DateTime date;

        #endregion

        #region Properties

        public string Data { get => data; set => data = value; }
        public string UserName { get => userName; set => userName = value; }
        public string SubscriberName { get => subscriberName; set => subscriberName = value; }
        public string Host { get => host; set => host = value; }
        public string Msg { get => msg; set => msg = value; }
        public bool Success { get => success; set => success = value; }
        public bool Ping { get => ping; set => ping = value; }
        public string Channel { get => channel; set => channel = value; }
        public List<string> NamesUsers { get => namesUsers; set => namesUsers = value; }
        public string Sign { get => sign; set => sign = value; }
        public int Subscription { get => subscription; set => subscription = value; }
        public bool VoteActive { get => voteActive; set => voteActive = value; }
        public string Theme { get => theme; set => theme = value; }
        public List<string> Variants { get => variants; set => variants = value; }
        public int AdvertTime { get => advertTime; set => advertTime = value; }
        public int AdvertCount { get => advertCount; set => advertCount = value; }
        public string Advert { get => advert; set => advert = value; }
        public string DeathCommand { get => deathCommand; set => deathCommand = value; }
        public string Vkid { get => vkid; set => vkid = value; }
        public int QuoteNumber { get => quoteNumber; set => quoteNumber = value; }
        public string Quote { get => quote; set => quote = value; }
        public string QuoteOperation { get => quoteOperation; set => quoteOperation = value; }
        public DateTime Date { get => date; set => date = value; }

        #endregion

        public Message(string data)
        {
            lock (data)
            {
                this.Data = data;

                var math = typeReg.Match(data);
                if (math.Success)
                    Success = Enum.TryParse(math.Groups["type"].Value, out Type);
                else
                {
                    math = pingReg.Match(data);
                    if (math.Success)
                    {
                        Type = TypeMessage.PING;
                        Success = true;
                    }
                    else
                    {
                        math = namesReg.Match(data);
                        if (math.Success)
                        {
                            string buf = "";
                            List<string> list = new List<string>();
                            foreach (var item in math.Groups["users"].Value)
                            {
                                if (item == ' ')
                                {
                                    list.Add(buf);
                                    buf = "";
                                }
                                else
                                    buf += item;
                            }
                            list.Add(buf);
                            Channel = math.Groups["channel"].Value;
                            NamesUsers = list;
                            Type = TypeMessage.NAMES;
                            Success = true;
                        }
                        else
                            Success = false;
                    }
                }

                if (Success == false)
                {
                    return;
                }
                switch (Type)
                {
                    case TypeMessage.PING:
                        Ping = true;
                        break;
                    case TypeMessage.JOIN:
                        math = joinOrpartReg.Match(data);
                        if (!math.Success)
                        {
                            Success = false;
                            break;
                        }
                        Channel = math.Groups["channel"].Value;
                        UserName = math.Groups["username"].Value;
                        break;
                    case TypeMessage.PART:
                        math = joinOrpartReg.Match(data);
                        if (!math.Success)
                        {
                            Success = false;
                            break;
                        }
                        Channel = math.Groups["channel"].Value;
                        UserName = math.Groups["username"].Value;
                        break;
                    case TypeMessage.MODE:
                        math = modeReg.Match(data);
                        if (math.Success)
                        {
                            UserName = math.Groups["username"].Value;
                            Channel = math.Groups["channel"].Value;
                            Sign = math.Groups["sign"].Value;
                        }
                        break;
                    case TypeMessage.NAMES:
                        if (math.Success && math.Groups["users"].Success && math.Groups["channel"].Success)
                        {
                            string users = "";
                            NamesUsers = new List<string>();
                            foreach (var item in math.Groups["users"].Value)
                            {
                                if (item != ' ')
                                    users += item;
                                else
                                {
                                    NamesUsers.Add(users);
                                    users = "";
                                }
                            }
                            NamesUsers.Add(users);
                            Channel = math.Groups["channel"].Value;
                        }
                        break;
                    case TypeMessage.NOTICE:
                        Success = false;
                        break;
                    case TypeMessage.HOSTTARGET:
                        Success = false;
                        break;
                    case TypeMessage.CLEARCHAT:
                        Success = false;
                        break;
                    case TypeMessage.USERSTATE:
                        Success = false;
                        break;
                    case TypeMessage.RECONNECT:
                        Success = false;
                        break;
                    case TypeMessage.ROOMSTATE:
                        Success = false;
                        break;
                    case TypeMessage.USERNOTICE:

                        math = usernoticeReg.Match(this.Data);
                        if (math.Success)
                        {
                            SubscriberName = math.Groups["username"].Value;
                            Subscription = int.Parse(math.Groups["sub"].Value);
                            Channel = math.Groups["channel"].Value;
                        }
                        else
                            Success = false;
                        break;
                    case TypeMessage.Tags:
                        Success = false;
                        break;
                    case TypeMessage.PRIVMSG:
                        math = pvmsgTagReg.Match(data);

                        if (!math.Success)
                        {
                            math = pvmsgReg.Match(data);
                            if (!math.Success)
                            {
                                Success = false;
                                break;
                            }
                        }

                        UserName = math.Groups["username"].Value;
                        Msg = math.Groups["msg"].Value;
                        Channel = math.Groups["channel"].Value;

                        if (UserName == "twitchnotify")
                        {
                            math = subscribeReg.Match(Msg);
                            if (math.Success)
                            {
                                SubscriberName = math.Groups["username"].Value;
                                Subscription = 1;
                            }
                            break;
                        }

                        math = commandReg.Match(data);
                        if (math.Success)
                            if (!Enum.TryParse(math.Groups["command"].Value, out Command))
                                Command = Command.unknown;

                        if (Msg.StartsWith("!vote"))
                        {
                            math = voteReg.Match(Msg);
                            if (math.Success)
                            {
                                Variants = new List<string>();
                                string buf = "";
                                foreach (var c in math.Groups["variants"].Value)
                                {
                                    if (c == ',')
                                    {
                                        Variants.Add(buf);
                                        buf = "";
                                    }
                                    else
                                    {
                                        buf += c;
                                    }
                                }
                                Variants.Add(buf);
                                Theme = math.Groups["theme"].Value;
                                Success = int.TryParse(math.Groups["time"].Value, out Time);
                                Command = Command.vote;
                                VoteActive = true;
                            }
                            else
                                Success = false;

                        }
                        else if(Msg.StartsWith("!deathbattle"))
                        {
                            math = deathBattleReg.Match(Msg);
                            if (math.Success)
                            {
                                DeathCommand = math.Groups["command"].Value;
                                Command = Command.deathbattle;
                            }
                            else
                                Success = false;
                        }
                        else if (Msg.StartsWith("!death"))
                        {
                            math = deathReg.Match(Msg);
                            if (math.Success)
                            {
                                DeathCommand = math.Groups["command"].Value;
                                Command = Command.death;
                            }
                            else
                                Success = false;
                        }
                        else if (Msg.StartsWith("!quote"))
                        {
                            if (Command == Command.quote)
                                break;
                            math = quoteShowReg.Match(Msg);
                            if (math.Success)
                            {
                                QuoteNumber = int.Parse(math.Groups["number"].Value);
                                Command = Command.quote;
                            }
                            else
                            {
                                math = quoteDateReg.Match(Msg);
                                if (math.Success)
                                {
                                    QuoteOperation = math.Groups["op"].Value;
                                    Quote = math.Groups["some"].Value;
                                    var s = math.Groups["date"].Value.Split('.');
                                    Date = new DateTime(int.Parse(s[2]), int.Parse(s[1]), int.Parse(s[0]));
                                    Command = Command.quote;
                                }                                  
                                else
                                {
                                    math = quoteReg.Match(Msg);
                                    if (math.Success)
                                    {
                                        QuoteOperation = math.Groups["op"].Value;
                                        Quote = math.Groups["some"].Value;
                                        Date = DateTime.Now;
                                        Command = Command.quote;
                                    }
                                    else
                                        Success = false;
                                }
                            }
                        }
                        else if(Msg.StartsWith("!qupdate"))
                        {
                            math = quoteUpdateWithDateReg.Match(Msg);
                            if (math.Success)
                            {
                                QuoteNumber = int.Parse(math.Groups["number"].Value);
                                Quote = math.Groups["quote"].Value;
                                var s = math.Groups["date"].Value.Split('.');
                                Date = new DateTime(int.Parse(s[2]), int.Parse(s[1]), int.Parse(s[0]));
                                Command = Command.qupdate;
                            }
                            else
                            {
                                math = quoteUpdateReg.Match(Msg);
                                if (math.Success)
                                {
                                    QuoteNumber = int.Parse(math.Groups["number"].Value);
                                    Quote = math.Groups["quote"].Value;
                                    Command = Command.qupdate;
                                }
                                else
                                    Success = false;
                            }
                            
                        }
                        else if (Msg.StartsWith("!advert"))
                        {
                            math = advertReg.Match(Msg);
                            if (math.Success)
                            {
                                AdvertTime = int.Parse(math.Groups["time"].Value);
                                AdvertCount = int.Parse(math.Groups["count"].Value);
                                Advert = math.Groups["advert"].Value;
                                Command = Command.advert;
                            }
                            else
                                Success = false;
                        }
                        else if (Msg.StartsWith("!vkid"))
                        {
                            math = vkidReg.Match(Msg);
                            if (math.Success)
                            {
                                Command = Command.vkid;
                                Vkid = math.Groups["id"].Value;
                            }
                            else
                                Success = false;
                        }


                        break;
                    case TypeMessage.GLOBALUSERSTATE:
                        break;
                    case TypeMessage.UNKNOWN:
                        break;
                    default:
                        Success = false;
                        break;
                }
            }
        }
    }
}
