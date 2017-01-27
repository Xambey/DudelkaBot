using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DudelkaBot.system
{
    class Channel
    {
        public string Name{ get; private set; }

        public List<User> Users { get; set; }
        public List<User> Moderators{ get; set; }

        Channel(string name)
        {
            
            Name = name;
            Users = new List<User>();
            Moderators = new List<User>();
        }
    }
}
