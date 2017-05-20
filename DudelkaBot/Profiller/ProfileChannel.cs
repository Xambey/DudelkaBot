using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DudelkaBot.Profiller
{
    public class ProfileChannel
    {
        #region Fields
        private Activities activities;
        private List<string> resubAnswers;
        private List<string> subAnswers;
        private string name;
        private int vote;
        private int advert;
        private int vkid;
        private int djid;
        private int qupdate;
        private int counter;
        private int quote;
        private int streamertime;
        private int help;
        private int members;
        private int mystat;
        private int toplist;
        private int moscowtime;
        private int music;
        private int viewers;
        private int uptime;
        private int ball8;
        private int reconnect;
        private int discord;
        private int wakeup;
        private int sleep;
        #endregion

        #region Properties
        public string Name { get => name; set => name = value; }
        public int Vote { get => vote; set => vote = value; }
        public int Advert { get => advert; set => advert = value; }
        public int Vkid { get => vkid; set => vkid = value; }
        public int Djid { get => djid; set => djid = value; }
        public int Qupdate { get => qupdate; set => qupdate = value; }
        public int Counter { get => counter; set => counter = value; }
        public int Quote { get => quote; set => quote = value; }
        public int Streamertime { get => streamertime; set => streamertime = value; }
        public int Help { get => help; set => help = value; }
        public int Members { get => members; set => members = value; }
        public int Mystat { get => mystat; set => mystat = value; }
        public int Toplist { get => toplist; set => toplist = value; }
        public int Moscowtime { get => moscowtime; set => moscowtime = value; }
        public int Music { get => music; set => music = value; }
        public int Viewers { get => viewers; set => viewers = value; }
        public int Uptime { get => uptime; set => uptime = value; }
        public List<string> SubAnswers { get => subAnswers; set => subAnswers = value; }
        public List<string> ResubAnswers { get => resubAnswers; set => resubAnswers = value; }
        public int Ball8 { get => ball8; set => ball8 = value; }
        public int Reconnect { get => reconnect; set => reconnect = value; }
        public int Discord { get => discord; set => discord = value; }
        public int Wakeup { get => wakeup; set => wakeup = value; }
        public int Sleep { get => sleep; set => sleep = value; }
        public Activities Activities { get => activities; set => activities = value; }

        private static Random rand = new Random();
        #endregion

        /// <summary>
        /// all enable
        /// </summary>
        public ProfileChannel(
            string name,
            string[] activities,
            int vote = 1, 
            int advert = 1, 
            int vkid = 1, 
            int djid = 1, 
            int qupdate = 1, 
            int counter = 1, 
            int quote = 1, 
            int moscowtime = 1,
            int help = 1,
            int members = 1,
            int mystat = 1,
            int toplist = 1,
            int streamertime = 1,
            int music = 1,
            int viewers = 1,
            int uptime = 1,
            int ball8 = 1,
            int reconnect = 1,
            int discord = 1,
            int wakeup = 1,
            int sleep = 1)
        {
            Name = name;
            Vote = vote > 0 ? 1 : 0;
            Advert = advert > 0 ? 1 : 0;
            Vkid = vkid > 0 ? 1 : 0;
            Djid = djid > 0 ? 1 : 0;
            Qupdate = qupdate > 0 ? 1 : 0;
            Counter = counter > 0 ? 1 : 0;
            Quote = quote > 0 ? 1 : 0;
            Moscowtime = moscowtime > 0 ? 1 : 0;
            Help = help > 0 ? 1 : 0;
            Members = members > 0 ? 1 : 0;
            Mystat = mystat > 0 ? 1 : 0;
            Toplist = toplist > 0 ? 1 : 0;
            Streamertime = streamertime > 0 ? 1 : 0;
            Music = music > 0 ? 1 : 0;
            Viewers = viewers > 0 ? 1 : 0;
            Uptime = uptime > 0 ? 1 : 0;
            Ball8 = ball8 > 0 ? 1 : 0;
            Reconnect = reconnect > 0 ? 1 : 0;
            Discord = discord > 0 ? 1 : 0;
            Wakeup = wakeup > 0 ? 1 : 0;
            Sleep = sleep > 0 ? 1 : 0;

            Activities = new Activities(activities);
        }

        public string GetRandomSubAnswer()
        {
            if (SubAnswers == null || SubAnswers.Count() == 0)
                return null;
            List<string> buf = new List<string>();
            string str = "";
            foreach (var item in SubAnswers)
            {
                if (string.IsNullOrEmpty(item))
                {
                    buf.Add(str);
                    str = "";
                }
                else
                    str += item + " ";
            }
            if (str.Length > 0)
            {
                buf.Add(str);
            }
            if (buf.Count() == 1)
                return buf.First();
            return buf.ElementAt(rand.Next(0, buf.Count()));
        }

        public string GetRandomResubAnswer()
        {
            if (ResubAnswers == null || ResubAnswers.Count() == 0)
                return null;
            List<string> buf = new List<string>();
            string str = "";
            foreach (var item in ResubAnswers)
            {
                if (string.IsNullOrEmpty(item))
                {
                    buf.Add(str);
                    str = "";
                }
                else
                    str += item + " ";
            }
            if(str.Length > 0)
            {
                buf.Add(str);
            }

            if (buf.Count() == 1)
                return buf.First();
            return buf.ElementAt(rand.Next(0, buf.Count()));
        }
    }
}
