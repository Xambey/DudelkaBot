using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DudelkaBot.dataBase.model
{
    public class Gamers
    {
        [Key, Required]
        public int Channel_ID { get; set; }
        [Key, Required]
        public int User_ID { get; set; }
        [DefaultValue(false)]
        public bool Played { get; set; }

        public Gamers() { }
        public Gamers(int Channel_ID, int User_ID)
        {
            this.Channel_ID = Channel_ID;
            this.User_ID = User_ID;
        }
    }
}
