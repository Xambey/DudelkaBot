using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DudelkaBot.dataBase.model
{
    public class SubDayVotes
    {
        [Key]
        public int Game_id { get; set; }
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Required]
        public int Number { get; set; }
        [Required]
        public string UserName { get; set; }

        public SubDayVotes() { }

        public SubDayVotes(string UserName, int GameId)
        {
            this.UserName = UserName;
            Game_id = GameId;
        }
    }
}
