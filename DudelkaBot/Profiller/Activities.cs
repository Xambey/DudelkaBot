using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DudelkaBot
{
    public class Activities
    {
        #region Fields
        int subscriptions;
        int resubscriptions;
        int showQuote;
        int checkSubscriptions;
        int showChangedMusic;
        string client_ID;
        string oauth;
        #endregion

        #region Properties
        public int Subscriptions { get => subscriptions; set => subscriptions = value; }
        public int Resubscriptions { get => resubscriptions; set => resubscriptions = value; }
        public int ShowQuote { get => showQuote; set => showQuote = value; }
        public string Client_ID { get => client_ID; set => client_ID = value; }
        public string Oauth { get => oauth; set => oauth = value; }
        public int CheckSubscriptions { get => checkSubscriptions; set => checkSubscriptions = value; }
        public int ShowChangedMusic { get => showChangedMusic; set => showChangedMusic = value; }

        private static string patternState = @"(?<command>\w+)\s*=\s*(?<value>.+)";
        private static Regex StateReg = new Regex(patternState);
        #endregion

        public Activities(string[] source)
        {
            var dir = new Dictionary<string, string>();
            foreach (var item in source)
            {
                var m = StateReg.Match(item);
                if (m.Success)
                {
                    dir.Add(m.Groups["command"].Value, m.Groups["value"].Value);
                }
            }

            Subscriptions = int.Parse(dir["Subscriptions"]) > 0 ? 1 : 0;
            Resubscriptions = int.Parse(dir["Resubscriptions"]) > 0 ? 1 : 0;
            ShowQuote = int.Parse(dir["ShowQuotes"]) > 0 ? 1 : 0;
            CheckSubscriptions = int.Parse(dir["CheckSubscriptions"]) > 0 ? 1 : 0;
            ShowChangedMusic = int.Parse(dir["ShowChangedMusic"]) > 0 ? 1 : 0;

            Client_ID = dir["Client_ID"];
            Oauth = dir["OAuth"];
        }

    }
}
