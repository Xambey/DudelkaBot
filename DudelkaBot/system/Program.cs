using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DudelkaBot.ircClient;
using System.Net.Sockets;
using DudelkaBot.dataBase;
using System.Text;
using System.Text.RegularExpressions;

namespace DudelkaBot.system
{

    public class Program
    {
        #region Variables
        static string userName = "DudelkaBot";
        static string password = "oauth:x2ha2yusryy5dir8xlhkg90rqfpkld";
        static string host = "irc.chat.twitch.tv";
        static int port = 6667;

        static List<string> channels_names = new List<string>()
        {
            "dudelka_krasnaya",
            //"c9sneaky",
            //"fairlight_excalibur",
            //"lirik",
            //"domingo",
            //"imaqtpie",
            //"lck1",
            //"silvername",
            "blackufa_twitch",
            "dariya_willis"
        };

        #endregion
        static string pattern = @"!(?<channel>\w+)";
        static Regex reg = new Regex(pattern);

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;

            //var db = new dataBase.model.ChatContext();

            //string name = "asdasd";
            //lock (db.Users)
            //{
            //    int userID = db.Users.Single(u => u.username == name).Id;
            //    foreach (var ch in db.ChannelsUsers.Where(c => c.id == userID))
            //        ch.countmessage = 0;
            //}

            //db.SaveChanges();

            foreach (var item in channels_names)
            {
                Channel channel = new Channel(item, host, port, userName, password);
                channel.Join();
            }
            Channel.channels.First().Value.startShow();

            while (true)
            {
                string cmd = Console.ReadLine();
                if (!Channel.ircClient.tcpClient.Connected)
                    Channel.ircClient.tcpClient.ConnectAsync(host, int.Parse(password)).Wait();
                switch (cmd)
                {
                    case "!Dariya":
                        Channel.channels["dariya_willis"].startShow();
                        break;
                    case "!Black":
                        Channel.channels["blackufa_twitch"].startShow();
                        break;
                    case "!update":
                        Channel.ircClient.updateMembers();
                        break;
                    case "!errors":
                        var color = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;
                        lock (Channel.errorListMessages)
                        {
                            var ch = Channel.errorListMessages;
                            var l = ch.Count;
                            for (int i = 0; i < l; i++)
                            { 
                                Console.WriteLine(ch[i].Data);
                            }
                        }
                        Console.ForegroundColor = color;

                        break;
                    default:
                        var math = reg.Match(cmd);
                        if (math.Success)
                        {
                            if (math.Groups["channel"].Success)
                            {
                                var channel = math.Groups["channel"].Value;
                                if (Channel.channels.Any(p => p.Key == channel))
                                {
                                    Channel.channels[channel].startShow();
                                }
                            }
                        }
                        break;
                }
            }
        }
    }
}

