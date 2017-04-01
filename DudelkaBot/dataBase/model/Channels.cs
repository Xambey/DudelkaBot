using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DudelkaBot.dataBase.model;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace DudelkaBot.dataBase.model
{
    public class Channels
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Channel_id { get; set; }
        public string Channel_name { get; set; }
        public int VkId { get; set; }

        public Channels() { }
        public Channels(string channelName)
        {
            Channel_name = channelName;
        }
    }
}
