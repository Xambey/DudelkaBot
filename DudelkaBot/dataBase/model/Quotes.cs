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
        public int Channel_id { get; set; }
        public int Number { get; set; }
        public DateTime Date { get; set; }
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
