using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DudelkaBot.dataBase.model
{
    public class SubDayGames
    {
        [Required]
        public int Channel_id { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity), Required]
        public int Game_id { get; set; }
        [Required]
        public string Name { get; set; }
        [DefaultValue(1), Required]
        public int Value { get; set; }

        public SubDayGames()
        {
        }

        public SubDayGames(string Name, int Channel_ID, int value = 1)
        {
            this.Channel_id = Channel_ID;
            this.Name = Name;
            Value = value;
        }

    }
}
