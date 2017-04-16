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
        private string name;
        private int vote;
        private int advert;
        private int vkid;
        private int djid;
        private int qupdate;
        private int counter;
        private int quote;
        private int sexylevel;
        private int date;
        private int help;
        private int members;
        private int mystat;
        private int toplist;
        private int citytime;
        private int music;
        private int viewers;
        private int uptime;
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
        public int Sexylevel { get => sexylevel; set => sexylevel = value; }
        public int Date { get => date; set => date = value; }
        public int Help { get => help; set => help = value; }
        public int Members { get => members; set => members = value; }
        public int Mystat { get => mystat; set => mystat = value; }
        public int Toplist { get => toplist; set => toplist = value; }
        public int Citytime { get => citytime; set => citytime = value; }
        public int Music { get => music; set => music = value; }
        public int Viewers { get => viewers; set => viewers = value; }
        public int Uptime { get => uptime; set => uptime = value; }
        #endregion

        /// <summary>
        /// all enable
        /// </summary>
        public ProfileChannel(
            string name,
            int vote = 1, 
            int advert = 1, 
            int vkid = 1, 
            int djid = 1, 
            int qupdate = 1, 
            int counter = 1, 
            int quote = 1, 
            int sexylevel = 1,
            int date = 1,
            int help = 1,
            int members = 1,
            int mystat = 1,
            int toplist = 1,
            int citytime = 1,
            int music = 1,
            int viewers = 1,
            int uptime = 1)
        {
            Name = name;
            Vote = vote > 0 ? 1 : 0;
            Advert = advert > 0 ? 1 : 0;
            Vkid = vkid > 0 ? 1 : 0;
            Djid = djid > 0 ? 1 : 0;
            Qupdate = qupdate > 0 ? 1 : 0;
            Counter = counter > 0 ? 1 : 0;
            Quote = quote > 0 ? 1 : 0;
            Sexylevel = sexylevel > 0 ? 1 : 0;
            Date = date > 0 ? 1 : 0;
            Help = help > 0 ? 1 : 0;
            Members = members > 0 ? 1 : 0;
            Mystat = mystat > 0 ? 1 : 0;
            Toplist = toplist > 0 ? 1 : 0;
            Citytime = citytime > 0 ? 1 : 0;
            Music = music > 0 ? 1 : 0;
            Viewers = viewers > 0 ? 1 : 0;
            Uptime = uptime > 0 ? 1 : 0;
        }
    }
}
