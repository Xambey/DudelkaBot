using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DudelkaBot.system
{
    class User
    {
        public string Name { get; private set; }
        public int points;
        public int countMessages;

        User(string name)
        {
            Name = name;
        }

    }
}
