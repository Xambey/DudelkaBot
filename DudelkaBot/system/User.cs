using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DudelkaBot.system
{
    public class User
    {
        public string UserName { get; private set; }
        public int CountMessage = 0;
        public int Subscription = 0;
        public User(string userName)
        {
            UserName = userName;
        }
    }
}
