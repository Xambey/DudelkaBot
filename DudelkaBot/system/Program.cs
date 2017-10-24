﻿using System;
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
using DudelkaBot.WebClients;
using System.Resources;

namespace DudelkaBot.system
{

    public class Program
    {
        #region Variables
        static string userName = /*"DudeJIkaBot";*/"DudelkaBot";
        static string password = /*"oauth:gqqqwtjj03paeehisajfojfpvapk33";*/"oauth:k1vf6fr82i4inavo2odnhuaq8d8rz2";
        static string host = "irc.chat.twitch.tv";//"199.9.253.119";//
        static int port = 6667;
        static string channelNamesPath = "./ProfileChannels/ChannelsNames.txt";
        #endregion`

        static List<string> channels_names = System.IO.File.ReadAllLines(channelNamesPath).ToList();

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
                    case "!load sub games":
                        string filename = Console.ReadLine();
                        string ch = Console.ReadLine();
                        if (!string.IsNullOrEmpty(filename) && !string.IsNullOrEmpty(ch))
                        {
                            var ds = Channel.Channels.FirstOrDefault(a => a.Key == ch).Value;
                            if(ds != null)
                            {
                                ds.UpdateDbGameVote(filename);
                            }
                        }
                        break;
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
                        foreach ( var item in Channel.Channels)
                        {
                            Channel.IrcClient.SendChatBroadcastMessage(m, item.Value.Name);
                        }
                        break;
                    case "!news":
                        string me = Console.ReadLine();
                        foreach (var item in Channel.Channels)
                        {
                            Channel.IrcClient.SendChatBroadcastMessage(@"/me" + me, item.Value.Name);
                        }
                        break;
                    case "!start show common log":
                        Logger.ShowLineCommonMessage("Запуск отображения общего лога");
                        Channel.ActiveLog = true;
                        break;
                    case "!stop show common log":
                        Logger.ShowLineCommonMessage("Остановка отображения общего лога");
                        Channel.ActiveLog = false;
                        break;
                    case "!whisper":
                        string mesa = Console.ReadLine();
                        string usernam = Console.ReadLine();
                        Channel.IrcClient.SendChatWhisperMessage(mesa,usernam,324, "dfdf");
                        break;
                    case "!status":
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("List channels:");
                        foreach (var item in Channel.Channels)
                        {
                            Console.WriteLine($"\tСтатус {item.Value.Name} чата: - {item.Value.StatusChat} | канала: {item.Value.StatusStream}");
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
                    case "!help":
                        Console.WriteLine("Command list:");
                        Console.WriteLine("\t!stop show channel log - Остановка отображения лога канала");
                        Console.WriteLine("\t!start show channel log - Запуск отображения лога канала");
                        Console.WriteLine("\t!reconnect - переподключение к серверу");
                        Console.WriteLine("\t!send ENTER [сообщение] ENTER [название канала] - отправить сообщение в чат");
                        Console.WriteLine("\t!broadcast ENTER [сообщение] - массовая рассылка сообщений");
                        Console.WriteLine("\t!news ENTER [сообщение] - массовая рассылка сообщений c подсветкой для уведомлений");
                        Console.WriteLine("\t!stop show common log - Остановка отображения общего лога");
                        Console.WriteLine("\t!start show common log - Запуск отображения общего лога");
                        Console.WriteLine("\t!whisper ENTER [сообщение] ENTER [имя получателя]");
                        Console.WriteLine("\t!status - статус каналов");
                        Console.WriteLine("\t!savelogs - сохранить все логи");
                        Console.WriteLine("\t!add ENTER [название канала] - подключить канал с отслеживанию");
                        Console.WriteLine("\t!remove ENTER [название канала] - отключить от отслеживания канал");
                        Console.WriteLine("\t![название канала] - включить отображение чата");
                        Console.WriteLine("\t!exit - выключение с сохранением логов");
                        Console.WriteLine("\t!load sub games - [путь к файлу] [название канала] - обновить список игр для саб. дня");
                        Thread.Sleep(10000);
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

