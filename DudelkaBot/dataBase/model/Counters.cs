using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace DudelkaBot.dataBase.model
{
    public class Counters
    {
        [Required]
        public int Channel_id { get; set; }
        [MaxLength, Required]
        public string Counter_name { get; set; }
        [DefaultValue(0), Required]
        public int Count { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity), Required]
        public int Number { get; set; }
        [MaxLength, Required]
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
