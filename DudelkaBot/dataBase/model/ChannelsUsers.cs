using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DudelkaBot.dataBase.model
{
    public class ChannelsUsers
    {
        [Key]
        public int User_id { get; set; }
        public int Channel_id { get; set; }
        public int CountMessage { get; set; }
        public int CountSubscriptions { get; set; }
    }
}
