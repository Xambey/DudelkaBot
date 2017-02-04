using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DudelkaBot.ircClient
{
    public class Message
    {
        private static string patternPVMSG = @":(?<username>\w+)!\w+@\w+.tmi.twitch.tv (?<type>\w+) #(?<channel>\w+) :(?<msg>.*)";
        private static string patternPARTorJOIN = @":(?<username>\w+)!\w+@\w+.tmi.twitch.tv (?<type>\w+) #(?<channel>\w+)";
        private static string typePattern = @" (?<type>[A-Z ]+) #";
        private static string commandPattern = @"#(?<channel>\w+) :!(?<command>\w+)$";
        private static string pingPattern = @"PING\s+";
        private static string namesPattern = @":\S+ (\d*) \S+\w+ = #(?<channel>\w+) :(?<user1>\w+) (?<user2>\w+) (?<user3>\w+)";
        private static string modePattern = @":.+ #(?<channel>\w+) (?<sign>.)o (?<username>\w+)";
        private static string usernoticePattern = @":.* #(?<channel>\w+) :(?<username>\w+) has subscribed for (?<sub>\d+) months in row!";

        private static Regex typeReg = new Regex(typePattern);
        private static Regex joinOrpartReg = new Regex(patternPARTorJOIN);
        private static Regex pvmsgReg = new Regex(patternPVMSG);
        private static Regex commandReg = new Regex(commandPattern);
        private static Regex pingReg = new Regex(pingPattern);
        private static Regex namesReg = new Regex(namesPattern);
        private static Regex modeReg = new Regex(modePattern);
        private static Regex usernoticeReg = new Regex(usernoticePattern);


        public string Data { get; private set; }
        public string UserName { get; private set; }
        public string Host { get; private set; }
        public TypeMessage Type = TypeMessage.UNKNOWN;
        public Command command = Command.unknown;
        public string Msg { get; private set; }
        public bool Success = true;
        public bool Ping = false;
        public string Channel { get; private set; }
        public string User1 { get; private set; }
        public string User2 { get; private set; }
        public string User3 { get; private set; }
        public string Sign { get; private set; }
        public int Subscription;


        public Message(string data)
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
                    User1 = math.Groups["user1"].Value;
                    User2 = math.Groups["user2"].Value;
                    User3 = math.Groups["user3"].Value;
                    Channel = math.Groups["channel"].Value;
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
                    math = usernoticeReg.Match(data);
                    if (math.Success)
                    {
                        UserName = math.Groups["username"].Value;
                        Channel = math.Groups["channel"].Value;
                        Success = int.TryParse(math.Groups["sub"].Value, out Subscription);
                    }
                    else
                        Success = false;
                    break;
                case TypeMessage.Tags:
                    Success = false;
                    break;
                case TypeMessage.PRIVMSG:
                    math = joinOrpartReg.Match(data);

                    if (!math.Success)
                    {
                        Success = false;
                        break;
                    }

                    UserName = math.Groups["username"].Value;
                    Msg = math.Groups["msg"].Value;
                    Channel = math.Groups["channel"].Value;

                    math = commandReg.Match(data);
                    if (math.Success)
                        Success = Enum.TryParse(math.Groups["command"].Value, out command);

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
