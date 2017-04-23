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
using DudelkaBot.Logging;

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
            //"lenagol0vach",
            //"dota2ruhub",
            //"thijshs",
            //"pgl_dota",
            //"kephrii",
            //"voyboy",
            //"lpl1"
        };
        static string pattern = @"!(?<channel>\w+)";
        static Regex reg = new Regex(pattern);

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;
            Console.InputEncoding = Encoding.Unicode;
            //!!!!!!!!!! 
            Channel.Port = port;
            Channel.Password = password;
            Channel.Iphost = host;
            Channel.UserName = userName;

            foreach (var item in channels_names)
            {
                Channel channel = new Channel(item);
                channel.JoinRoom();
            }
            Channel.Channels.First().Value.StartShow();
            while (true)
            {
                Channel.IrcClient.isConnect();
                string cmd = Console.ReadLine();

                switch (cmd)
                {
                    case "!stop show channel log":
                        Logger.ShowLineCommonMessage("Остановка отображения лога канала");
                        Channel.Channels.First().Value.StopShow();
                        break;
                    case "!start show channel log":
                        Logger.ShowLineCommonMessage("Запуск отображения лога канала");
                        Channel.Channels.First().Value.StopShow();
                        break;
                    case "!reconnect":
                        Logger.ShowLineCommonMessage("Запуск переподключения к серверу...");
                        Channel.Reconnect();
                        break;
                    case "!Dariya":
                        Channel.Channels["dariya_willis"].StartShow();
                        break;
                    case "!Black":
                        Channel.Channels["blackufa_twitch"].StartShow();
                        break;
                    case "!send":
                        string mes = Console.ReadLine();
                        string o = Console.ReadLine();
                        if (string.IsNullOrEmpty(o) && Channel.ViewChannel != null)
                            o = Channel.ViewChannel.Name;
                        if(Channel.Channels.Any(a => a.Key == o))
                            Channel.IrcClient.SendChatBroadcastMessage(mes, o);
                        break;
                    case "!broadcast":
                        string m = Console.ReadLine();
                        if (Channel.ViewChannel == null)
                            break;
                        if (Channel.Channels.Any(a => a.Key == Channel.ViewChannel.Name))
                            Channel.IrcClient.SendChatBroadcastMessage(m,Channel.ViewChannel.Name);
                        break;
                    case "!start show common log":
                        Logger.ShowLineCommonMessage("Запуск отображения общего лога");
                        Channel.ActiveLog = true;
                        break;
                    case "!stop show common log":
                        Logger.ShowLineCommonMessage("Остановка отображения общего лога");
                        Channel.ActiveLog = false;
                        break;
                    case "!errors":
                        Console.ForegroundColor = ConsoleColor.Red;
                        lock (Channel.ErrorListMessages)
                        {
                            var ch = Channel.ErrorListMessages;
                            var l = ch.Count;
                            for (int i = 0; i < l; i++)
                            { 
                                Console.WriteLine(ch[i].Data);
                            }
                        }
                        Console.ForegroundColor = ConsoleColor.Gray;

                        break;
                    case "!status":
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("List channels:");
                        foreach (var item in Channel.Channels)
                        {
                            Console.WriteLine($"Статус {item.Value.Name} чата: - {item.Value.StatusChat} | канала: {item.Value.StatusStream}");
                        }
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Thread.Sleep(2000);
                        break;
                    case "!savelogs":
                        foreach (var item in Channel.Channels)
                        {
                            Logger.SaveChannelLog(item.Value.Name);
                        }
                        Logger.SaveCommonLog();
                        Logger.ShowLineCommonMessage("Все логи сохранены!");
                        break;
                    case "!exit":
                        foreach (var item in Channel.Channels)
                        {
                            Logger.SaveChannelLog(item.Value.Name);
                        }
                        Logger.SaveCommonLog();
                        Environment.Exit(0);
                        break;
                    case "!add":
                        string chname = Console.ReadLine();
                        var chan = Channel.Channels.SingleOrDefault(a => a.Key == chname).Value;
                        if (chan == null) {
                            chan = new Channel(chname);
                            chan.JoinRoom();
                            chan.StartShow();
                            Logger.ShowLineCommonMessage($"Канал {chname} добавлен в список каналов!");
                        }
                        
                        break;
                    case "!remove":
                        string name = Console.ReadLine();
                        if (Channel.Channels.Any(a => a.Key == name))
                        {
                            Channel.Channels.Remove(name);
                            Logger.ShowLineCommonMessage($"Канал {name} удален из списка каналов!");
                        }
                        break;
                    default:
                        var math = reg.Match(cmd);
                        if (math.Success)
                        {
                            if (math.Groups["channel"].Success)
                            {
                                var channel = math.Groups["channel"].Value;
                                if (Channel.Channels.Any(p => p.Key == channel))
                                {
                                    Channel.Channels[channel].StartShow();
                                }
                            }
                        }
                        break;
                }
            }
        }
    }
}

