using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DudelkaBot.dataBase.model
{
    public class ChatContext : DbContext
    {
        public DbSet<Users> Users { get; set; }
        public DbSet<Channels> Channels { get; set; }
        public DbSet<ChannelsUsers> ChannelsUsers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=DudelkaBotBase;Trusted_Connection=True;");
        }
    }
}