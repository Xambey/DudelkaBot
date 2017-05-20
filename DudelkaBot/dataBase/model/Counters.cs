using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace DudelkaBot.dataBase.model
{
    public class Counters
    {
        public int Channel_id { get; set; }
        public string Counter_name { get; set; }
        public int Count { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Number { get; set; }
        public string Description { get; set; }

        public Counters() { }
        public Counters(int id_channel, string counter_name)
        {
            Channel_id = id_channel;
            Counter_name = counter_name;
            Count = 0;
            Description = string.Empty;
        }
        public Counters(int id_channel, string counter_name, string description) : this(id_channel,counter_name)
        {
            Description = description;
        }
    }
}
