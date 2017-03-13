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
        static string userName = /*"DudeJIkaBot";*/"DudelkaBot";
        static string password = /*"oauth:gqqqwtjj03paeehisajfojfpvapk33";*/"oauth:k1vf6fr82i4inavo2odnhuaq8d8rz2";
        static string host = "irc.chat.twitch.tv";//"199.9.253.119";//
        static int port = 6667;
        #endregion`

        static List<string> channels_names = new List<string>()
        {
            "dudelka_krasnaya",
            "blackufa_twitch",
            "dariya_willis"
            //"c9sneaky",
            //"nl_kripp",
            //"fairlight_excalibur",
            //"lirik",
            //"domingo",
            //"imaqtpie",
            //"lck1",
            //"silvername",
            //"lenagol0vach"
        };
        static string pattern = @"!(?<channel>\w+)";
        static Regex reg = new Regex(pattern);

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;
            Console.InputEncoding = Encoding.Unicode;

            foreach (var item in channels_names)
            {
                Channel channel = new Channel(item, host, port, userName, password);
                channel.Join();
            }
            Channel.channels.First().Value.StartShow();

            while (true)
            {
                Channel.ircClient.isConnect();
                string cmd = Console.ReadLine();

                switch (cmd)
                {
                    case "!stop":
                        Channel.channels.First().Value.StopShow();
                        break;
                    case "!start":
                        Channel.channels.First().Value.StopShow();
                        break;
                    case "!reconnect":
                        Channel.Reconnect();
                        break;
                    case "!Dariya":
                        Channel.channels["dariya_willis"].StartShow();
                        break;
                    case "!Black":
                        Channel.channels["blackufa_twitch"].StartShow();
                        break;
                    case "!send":
                        string mes = Console.ReadLine();
                        string o = Console.ReadLine();
                        if (string.IsNullOrEmpty(o) && Channel.viewChannel != null)
                            o = Channel.viewChannel.Name;
                        if(Channel.channels.Any(a => a.Key == o))
                            Channel.ircClient.SendChatBroadcastMessage(mes, o);
                        break;
                    case "!errors":
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
                        Console.ResetColor();

                        break;
                    case "!list":
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("List channels:");
                        foreach (var item in Channel.channels)
                        {
                            Console.WriteLine(item.Key + " " + item.Value.Status.ToString());
                        }
                        Console.ResetColor();
                        Thread.Sleep(2000);
                        break;
                    case "!add":
                        string chname = Console.ReadLine();
                        var chan = Channel.channels.SingleOrDefault(a => a.Key == chname).Value;
                        if (chan == null) {
                            chan = new Channel(chname, host, port, userName, password);
                            chan.Join();
                            chan.StartShow();
                        }
                        break;
                    case "!remove":
                        string name = Console.ReadLine();
                        if (Channel.channels.Any(a => a.Key == name))
                        {
                            Channel.channels.Remove(name);
                        }
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
                                    Channel.channels[channel].StartShow();
                                }
                            }
                        }
                        break;
                }
            }
        }
    }
}

