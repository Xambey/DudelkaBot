using DudelkaBot.enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DudelkaBot.Messages
{
    public class Message : IDisposable
    {
        #region Patterns
        private static string patternPVMSG = @":(?<username>\w+)!\w+@\w+.tmi.twitch.tv (?<type>\w+) #(?<channel>\w+) :(?<msg>.*)";
        private static string patternPRIVMSGtag = @"@.* :(?<username>\w+)!.* #(?<channel>\w+) :(?<msg>.*)";
        private static string patternPARTorJOIN = @":(?<username>\w+)!.+ (?<type>\w+) #(?<channel>\w+)";
        private static string typePattern = @" (?<type>[A-Z]+) #";
        private static string commandPattern = @"#(?<channel>\w+) :!(?<command>.+)$";
        private static string pingPattern = @"PING\s+";
        private static string namesPattern = @":\S+ \d+ \S+\w+ = #(?<channel>\w+) :(?<users>.*)";
        private static string modePattern = @":.+ #(?<channel>\w+) (?<sign>.)o (?<username>\w+)";
        private static string usernoticePattern = @".*login=(?<username>\w+).+msg-param-months=(?<sub>\d+).+system-msg=(?<message>[^;]+);.* USERNOTICE #(?<channel>\w+)";
        private static string subscribePattern = @"(?<username>\w+).+";
        private static string votePattern = @"!vote (?<theme>.+):(?<time>\d+):(?<variants>.+)";
        private static string advertPattern = @"!advert (?<time>\d+) (?<count>\d+) (?<advert>.+)";
        private static string vkidPattern = @"!vkid (?<id>\w+)$";
        private static string quotePattern = @"!quote\s+(?<op>[+-])\s+(?<some>.+)";
        private static string quoteDatePapptern = @"!quote\s+(?<op>[+-])\s+(?<date>\d{1,2}\.\d{1,2}\.\d{4})\s+(?<some>.+)";
        private static string quoteShowPattern = @"!quote (?<number>\d+)$";
        private static string quoteUpdatePattern = @"!qupdate (?<number>\d+) (?<quote>.+)";
        private static string quoteUpdateWithDate = @"!qupdate\s+(?<number>\d+)\s+(?<date>\d{1,2}\.\d{1,2}\.\d{4})\s+(?<quote>.+)";
        private static string counterPattern = @"!counter\s+(?<command>[+-])\s+(?<name>\w+)$";
        private static string existedCounterPattern = @"!(?<name>\w+)\s+(?<command>v|[+-]|\d+)$";
        private static string djidPattern = @"!djid (?<id>\w+)$";
        private static string ball8Pattern = @"!8ball\s+.+";
        #endregion

        #region Regexes
        private static Regex typeReg = new Regex(typePattern);
        private static Regex joinOrpartReg = new Regex(patternPARTorJOIN);
        private static Regex pvmsgTagReg = new Regex(patternPRIVMSGtag);
        private static Regex pvmsgReg = new Regex(patternPVMSG);
        private static Regex commandReg = new Regex(commandPattern);
        private static Regex pingReg = new Regex(pingPattern);
        private static Regex namesReg = new Regex(namesPattern);
        private static Regex modeReg = new Regex(modePattern);
        private static Regex usernoticeReg = new Regex(usernoticePattern);
        private static Regex subscribeReg = new Regex(subscribePattern);
        private static Regex voteReg = new Regex(votePattern);
        private static Regex advertReg = new Regex(advertPattern);
        private static Regex vkidReg = new Regex(vkidPattern);
        private static Regex quoteReg = new Regex(quotePattern);
        private static Regex quoteDateReg = new Regex(quoteDatePapptern);
        private static Regex quoteShowReg = new Regex(quoteShowPattern);
        private static Regex quoteUpdateReg = new Regex(quoteUpdatePattern);
        private static Regex quoteUpdateWithDateReg = new Regex(quoteUpdateWithDate);
        private static Regex counterReg = new Regex(counterPattern);
        private static Regex existedCounterReg = new Regex(existedCounterPattern);
        private static Regex djidReg = new Regex(djidPattern);
        private static Regex ball8Reg = new Regex(ball8Pattern);
        #endregion

        #region Fields
        public TypeMessage Type = TypeMessage.UNKNOWN;
        public Command Command = Command.unknown;
        public int Time = 0;

        private string djid;
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
        private string vkid;
        private int quoteNumber = 0;
        private string quote;
        private string quoteOperation;
        private DateTime date;
        private string oldName;
        private string newName;

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
        public string Vkid { get => vkid; set => vkid = value; }
        public int QuoteNumber { get => quoteNumber; set => quoteNumber = value; }
        public string Quote { get => quote; set => quote = value; }
        public string QuoteOperation { get => quoteOperation; set => quoteOperation = value; }
        public DateTime Date { get => date; set => date = value; }
        public string NewName { get => newName; set => newName = value; }
        public string OldName { get => oldName; set => oldName = value; }
        public string Djid { get => djid; set => djid = value; }

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

                        //if (UserName == "twitchnotify")
                        //{
                        //    math = subscribeReg.Match(Msg);
                        //    if (math.Success)
                        //    {
                        //        SubscriberName = math.Groups["username"].Value;
                        //        Subscription = 1;
                        //    }
                        //    if (Data.Contains("while\\syou\\swere\\saway!"))
                        //        Success = false;
                        //    break;
                        //}

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
                        else if(Msg.StartsWith("!counter"))
                        {
                            math = counterReg.Match(Msg);
                            if (math.Success)
                            {
                                Sign = math.Groups["command"].Value;
                                NewName = math.Groups["name"].Value;
                                Command = Command.counter;
                            }
                            else
                            {
                                if(Msg == "!counter")
                                {
                                    Command = Command.counter;
                                }
                                else
                                    Success = false;
                            }
                        }
                        else if (Msg.StartsWith("!quote"))
                        {
                            if(Msg == "!quote")
                            {
                                Command = Command.quote;
                                break;
                            }
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
                        else if (Msg.StartsWith("!djid"))
                        {
                            math = djidReg.Match(Msg);
                            if (math.Success)
                            {
                                Command = Command.djid;
                                Djid = math.Groups["id"].Value;
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
                        else if (Msg.StartsWith("!"))
                        {
                            math = existedCounterReg.Match(Msg);
                            if (math.Success)
                            {
                                NewName = math.Groups["name"].Value;
                                Sign = math.Groups["command"].Value;
                                Command = Command.existedcounter;
                            }
                            else
                            {
                                math = commandReg.Match(data);
                                if (math.Success)
                                {
                                    if (!Enum.TryParse(math.Groups["command"].Value, out Command)) { 
                                        Command = Command.unknown;
                                        math = ball8Reg.Match(Msg);
                                        if (math.Success)
                                        {
                                            Command = Command.ball;
                                        }
                                    }
                                }
                                else
                                    Success = false;
                            }
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

        public void Dispose()
        {
        }
    }
}
