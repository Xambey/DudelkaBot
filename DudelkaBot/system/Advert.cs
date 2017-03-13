using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DudelkaBot.system
{
    public class Advert
    {
        public static List<Advert> Adverts = new List<Advert>();
        private int count;
        private int time;
        public string advert { get; private set; }
        public string channel { get; private set; }
        private Timer timer;

        public Advert(int Time, int Count, string Advert, string Channel)
        {
            time = Time;
            count = Count;
            advert = Advert;
            channel = Channel;
            timer = new Timer(tickAdvert, null, time * 60000, time * 60000);
            Adverts.Add(this);
        }

        private void tickAdvert(object obj)
        {
            if (--count >= 0)
            {
                Channel.ircClient.SendChatBroadcastMessage("/me " + advert, channel);
            }
            else
                Adverts.Remove(this);
        }
    }
}
