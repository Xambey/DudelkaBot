using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DudelkaBot.Logging;

namespace DudelkaBot
{
    public static class Profiller
    {
        private static ConcurrentBag<ProfileChannel> profileChannels = new ConcurrentBag<ProfileChannel>();
        private static string templateCommandsPath = "./ProfileChannels/Commands/Template.txt";
        private static string templateSubAnswersPath = "./ProfileChannels/SubAnswers/Template.txt";
        private static string templateResubAnswersPath = "./ProfileChannels/ResubAnswers/Template.txt";
        private static string templateActivitiesPath = "./ProfileChannels/Activities/Template.txt";
        private static string patternState = @"!(?<command>\w+)\s*=\s*(?<value>\d+)";
        private static Regex StateReg = new Regex(patternState);

        static Profiller()
        {
            LoadProfiles();
        }
        private static void LoadProfiles()
        {
            var filenames = Directory.GetFiles("./ProfileChannels/Commands");

            foreach (var item in filenames)
            {
                Console.WriteLine(Path.GetFileNameWithoutExtension(item));
                profileChannels.Add(FileToProfileChannel(Path.GetFileNameWithoutExtension(item)));
            }
        }
        private static ProfileChannel FileToProfileChannel(string channelname)
        {
            string[] buf;
            buf = File.ReadAllText($"./ProfileChannels/Commands/{channelname}.txt").Split(new string[] { Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);

            var dir = new Dictionary<string, int>();

            foreach (var item in buf)
            {
                var m = StateReg.Match(item);
                if (m.Success)
                {
                    dir.Add(m.Groups["command"].Value, int.Parse(m.Groups["value"].Value));
                }
            }

            string[] sub = File.Exists($"./ProfileChannels/SubAnswers/{channelname}.txt") ? File.ReadAllLines($"./ProfileChannels/SubAnswers/{channelname}.txt") : null;
            if(sub == null)
            {
                File.Copy(templateSubAnswersPath, $"./ProfileChannels/SubAnswers/{channelname}.txt");
                sub = File.ReadAllLines($"./ProfileChannels/SubAnswers/{channelname}.txt");
            }
            string[] resub = File.Exists($"./ProfileChannels/ResubAnswers/{channelname}.txt") ? File.ReadAllLines($"./ProfileChannels/ResubAnswers/{channelname}.txt") : null;
            if(resub == null)
            {
                File.Copy(templateResubAnswersPath, $"./ProfileChannels/ResubAnswers/{channelname}.txt");
                resub = File.ReadAllLines($"./ProfileChannels/ResubAnswers/{channelname}.txt");
            }

            string[] activities = File.Exists($"./ProfileChannels/Activities/{channelname}.txt") ? File.ReadAllLines($"./ProfileChannels/Activities/{channelname}.txt") : null;
            if (activities == null)
            {
                File.Copy(templateActivitiesPath, $"./ProfileChannels/Activities/{channelname}.txt");
                activities = File.ReadAllLines($"./ProfileChannels/Activities/{channelname}.txt");
            }

            return new ProfileChannel(channelname,activities,dir["vote"],dir["advert"],dir["vkid"],dir["djid"],dir["qupdate"],dir["counter"],dir["quote"],dir["moscowtime"],dir["help"],dir["members"],dir["mystat"],dir["toplist"],dir["streamertime"],dir["music"],dir["viewers"],dir["uptime"],dir["8ball"],dir["reconnect"],dir["discord"],dir["wakeup"],dir["sleep"]) { SubAnswers = sub?.ToList(), ResubAnswers = resub?.ToList() };
        }
        public static bool TryCreateProfile(string channelname)
        {
            try
            {
                if (!File.Exists($"./ProfileChannels/Commands/{channelname}.txt") && File.Exists(templateCommandsPath))
                {
                    File.Copy(templateCommandsPath, $"./ProfileChannels/Commands/{channelname}.txt");

                    if (!File.Exists($"./ProfileChannels/SubAnswers/{channelname}.txt") && File.Exists(templateSubAnswersPath))
                        File.Copy(templateSubAnswersPath, $"./ProfileChannels/SubAnswers/{channelname}.txt");
                    if (!File.Exists($"./ProfileChannels/ResubAnswers/{channelname}.txt") && File.Exists(templateResubAnswersPath))
                        File.Copy(templateResubAnswersPath, $"./ProfileChannels/ResubAnswers/{channelname}.txt");
                    if (!File.Exists($"./ProfileChannels/Activities/{channelname}.txt") && File.Exists(templateActivitiesPath))
                        File.Copy(templateActivitiesPath, $"./ProfileChannels/ResubAnswers/{channelname}.txt");
                    profileChannels.Add(FileToProfileChannel(channelname));
                    return true;
                }
                else
                {
                    if (!File.Exists($"./ProfileChannels/SubAnswers/{channelname}.txt") && File.Exists(templateSubAnswersPath))
                        File.Copy(templateSubAnswersPath, $"./ProfileChannels/SubAnswers/{channelname}.txt");
                    if (!File.Exists($"./ProfileChannels/ResubAnswers/{channelname}.txt") && File.Exists(templateResubAnswersPath))
                        File.Copy(templateResubAnswersPath, $"./ProfileChannels/ResubAnswers/{channelname}.txt");
                    if (!File.Exists($"./ProfileChannels/Activities/{channelname}.txt") && File.Exists(templateActivitiesPath))
                        File.Copy(templateActivitiesPath, $"./ProfileChannels/ResubAnswers/{channelname}.txt");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Logger.ShowLineCommonMessage(ex.Message + ex.Data + ex.StackTrace);
                if (ex.InnerException != null)
                    Logger.ShowLineCommonMessage(ex.InnerException.Message + ex.InnerException.Data + ex.InnerException.StackTrace);
                Console.ForegroundColor = ConsoleColor.Gray;
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

