using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DudelkaBot.system
{
    public static class Profiller
    {
        private static ConcurrentBag<ProfileChannel> profileChannels = new ConcurrentBag<ProfileChannel>();
        private static string TemplateFilePath = "./ProfileChannels/Template.txt";
        private static string patternState = @"!(?<command>\w+)\s*=\s*(?<value>\d+)";
        private static Regex StateReg = new Regex(patternState);

        static Profiller()
        {
            LoadProfiles();
        }
        private static void LoadProfiles()
        {
            var filenames = Directory.GetFiles("./ProfileChannels/");

            foreach (var item in filenames)
            {
                profileChannels.Add(FileToProfileChannel(Path.GetFileNameWithoutExtension(item)));
            }
        }
        private static ProfileChannel FileToProfileChannel(string channelname)
        {
            string[] buf;
            if (channelname != TemplateFilePath)
                buf = File.ReadAllText($"./ProfileChannels/{channelname}.txt").Split(separator: new string[] { "\r\n" } , options: StringSplitOptions.RemoveEmptyEntries);
            else
                buf = File.ReadAllText(channelname).Split(separator: new string[] { "\r\n" }, options: StringSplitOptions.RemoveEmptyEntries);
            var dir = new Dictionary<string, int>();
            foreach (var item in buf)
            {
                var m = StateReg.Match(item);
                if (m.Success)
                {
                    dir.Add(m.Groups["command"].Value, int.Parse(m.Groups["value"].Value));
                }
            }
            return new ProfileChannel(channelname, dir["vote"], dir["advert"], dir["vkid"], dir["djid"], dir["qupdate"], dir["counter"], dir["quote"], dir["sexylevel"], dir["date"], dir["help"], dir["members"], dir["mystat"], dir["toplist"], dir["citytime"], dir["music"], dir["viewers"], dir["uptime"]);
        }
        public static bool TryCreateProfile(string channelname)
        {
            try
            {
                if (!File.Exists($"./ProfileChannels/{channelname}.txt") && File.Exists(TemplateFilePath))
                {
                    File.Copy(TemplateFilePath, $"./ProfileChannels/{channelname}.txt");
                    profileChannels.Add(FileToProfileChannel(channelname));
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Logger.ShowLineCommonMessage(ex.Message + ex.Data + ex.StackTrace);
                if (ex.InnerException != null)
                    Logger.ShowLineCommonMessage(ex.InnerException.Message + ex.InnerException.Data + ex.InnerException.StackTrace);
                Console.ResetColor();
                return false;
            }
        }
        public static ProfileChannel GetProfileOrDefault(string channelname)
        {
            var v = profileChannels.FirstOrDefault(a => a.Name == channelname);
            if(v != null)
                return v;
            return null;
        }
    }
}

