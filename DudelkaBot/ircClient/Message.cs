﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace DudelkaBot.ircClient
{
    public class Message
    {
        private static string patternPVMSG = @":(?<username>\w+)!\w+@\w+.tmi.twitch.tv (?<type>\w+) #(?<channel>\w+) :(?<msg>.*)";
        private static string patternPARTorJOIN = @":(?<username>\w+)!.+ (?<type>\w+) #(?<channel>\w+)";
        private static string typePattern = @" (?<type>[A-Z]+) #";
        private static string commandPattern = @"#(?<channel>\w+) :!(?<command>\w+)$";
        private static string pingPattern = @"PING\s+";
        private static string namesPattern = @":\S+ \d+ \S+\w+ = #(?<channel>\w+) :(?<users>.*)";
        private static string modePattern = @":.+ #(?<channel>\w+) (?<sign>.)o (?<username>\w+)";
        private static string usernoticePattern = @".+login=(?<username>\w+).+msg-param-months=(?<sub>\d+).* USERNOTICE #(?<channel>\w+)";
        private static string subscribePattern = @"(?<username>\w+).+";
        private static string votePattern = @"!vote (?<theme>.+):(?<time>\d+):(?<variants>.+)";

        private static string patternPRIVMSGtag = @"@.* :(?<username>\w+)!.* #(?<channel>\w+) :(?<msg>.*)";

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

        public string Data { get; private set; }
        public string UserName { get; private set; }
        public string SubscriberName { get; private set; }
        public string Host { get; private set; }
        public TypeMessage Type = TypeMessage.UNKNOWN;
        public Command command = Command.unknown;
        public string Msg { get; private set; }
        public bool Success = true;
        public bool Ping = false;
        public string Channel { get; private set; }
        public List<string> NamesUsers { get; private set; }
        public string Sign { get; private set; }
        public int Subscription = 0;
        public bool VoteActive = false;
        public string Theme;
        public List<string> variants;
        public int Time = 0;


        public Message(string data)
        {
            lock (data)
            {
                Data = data;

                var math = typeReg.Match(data);
                if (math.Success)
                    Success = Enum.TryParse(math.Groups["type"].Value, out Type);
                else
                {
                    Success = false;
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

                        math = usernoticeReg.Match(Data);
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
                            if (!Enum.TryParse(math.Groups["command"].Value, out command))
                                command = Command.unknown;

                        if (Msg.Contains("!vote"))
                        {
                            math = voteReg.Match(Msg);
                            if (math.Success)
                            {
                                variants = new List<string>();
                                string buf = "";
                                foreach (var c in math.Groups["variants"].Value)
                                {
                                    if (c == ',')
                                    {
                                        variants.Add(buf);
                                        buf = "";
                                    }
                                    else
                                    {
                                        buf += c;
                                    }
                                }
                                variants.Add(buf);
                                Theme = math.Groups["theme"].Value;
                                Success = int.TryParse(math.Groups["time"].Value, out Time);
                                command = Command.vote;
                                VoteActive = true;
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
