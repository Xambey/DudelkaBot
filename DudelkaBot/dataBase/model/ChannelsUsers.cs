using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DudelkaBot.dataBase.model
{
    public class ChannelsUsers
    {
        [Required]
        public int User_id { get; set; }
        [Required]
        public int Channel_id { get; set; }
        [Required]
        public bool Active { get; set; }
        [Required]
        public int CountMessage { get; set; }
        [Required]
        public int CountSubscriptions { get; set; }
        [Required]
        public bool Moderator { get; set; }

        public ChannelsUsers(){ } 
        public ChannelsUsers(int userId, int channel_id)
        {
            User_id = userId;
            Channel_id = channel_id;
            CountMessage = 0;
            CountSubscriptions = 0;
            Active = true;
        }
        public ChannelsUsers(int userId, int channel_id, int countSub)
        {
            User_id = userId;
            Channel_id = channel_id;
            CountMessage = 0;
            CountSubscriptions = countSub;
            Active = true;
        }
    }
}
