using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DudelkaBot.dataBase.model
{
    public class Channels
    {
        [Key] 
        public int Channel_id { get; set; }
        public string Channel_name { get; set; }
    }
}
