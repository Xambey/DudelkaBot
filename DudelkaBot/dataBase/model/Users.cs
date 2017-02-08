using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DudelkaBot.dataBase.model
{
    public class Users
    {
        [Key]
        public int Id { get; set; }
        public string username { get; set; }
    }
}
