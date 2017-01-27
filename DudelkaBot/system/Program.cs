﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IRCClient;
using System.Net.Sockets;
using System.Threading;

namespace DudelkaBot.system
{ 

    class Program
    {
        #region Variables
        static string userName = "DudelkaBot";
        static string password = "oauth:x2ha2yusryy5dir8xlhkg90rqfpkld";
        static string channelName = "opexalex";

        
        static Thread chatThread;
        static IrcClient ircClient = new IrcClient("irc.chat.twitch.tv", 6667, userName, password);
        static NetworkStream serverStream = ircClient.tcpClient.GetStream();
        static string readData = "";
        #endregion

        static List<string> commands = new List<string>()
        {
            "!hi-приветствие",
            "!date-дата и время сервера",
            "!commands-список команд"
        };

        static void Main(string[] args)
        {
            ircClient.joinRoom(channelName);
            chatThread = new Thread(new ThreadStart(Process));
            chatThread.Start();
        }

        private static void Process()
        {
            while (true)
            {
                try
                {
                    readData = ircClient.readMessage().Result;

                    if (string.IsNullOrEmpty(readData))
                        continue;

                    if (readData == "PING :tmi.twitch.tv")
                    {
                        ircClient.pingResponse();
                        continue;
                    }
                    else if (ircClient.senderName == "moobot" || ircClient.senderName == "nightbot") 
                        continue;

                    int pos = readData.LastIndexOf(':');
                    readData = new string(readData.Skip(pos + 1).ToArray()); //get message


                    switch (readData)
                    {
                        case "!hi":
                            ircClient.sendChatMessage("Hello, I'm DudelkaBot");
                            break;
                        case "!date":
                            ircClient.sendChatMessage(DateTime.Now.ToString());
                            break;
                        case "!commands":
                            ircClient.sendChatBroadcastChatMessage(commands);
                            break;
                        
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.ReadLine();
                    return;
                }
            }
        }
    }
}
