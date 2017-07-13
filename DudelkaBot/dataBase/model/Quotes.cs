using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DudelkaBot.dataBase.model
{
    public class Quotes
    {
        [Required]
        public int Channel_id { get; set; }
        [Required]
        public int Number { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [MaxLength, Required]
        public string Quote { get; set; }

        public Quotes() { }
        public Quotes(int channel_id, string quote, DateTime date)
        {
            Channel_id = channel_id;
            Quote = quote;
            Date = date.Date;
        }
        public Quotes(int channel_id, string quote, DateTime date, int num) : this(channel_id, quote, date)
        {
            Number = num;
        }
    }
}
