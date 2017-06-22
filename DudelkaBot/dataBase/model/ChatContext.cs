using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DudelkaBot.Logging;

namespace DudelkaBot.dataBase.model
{
    public class ChatContext : DbContext, IDisposable
    {
        public DbSet<Users> Users { get; set; }
        public DbSet<Channels> Channels { get; set; }
        public DbSet<ChannelsUsers> ChannelsUsers { get; set; }
        public DbSet<Quotes> Quotes { get; set; }
        public DbSet<Counters> Counters { get; set; }
        public DbSet<SubDayGames> SubDayGames { get; set; }
        public DbSet<SubDayVotes> SubDayVotes { get; set; }

        public ChatContext() : base() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            try
            {
                optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=DudelkaBotBase;Trusted_Connection=True;");
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Logger.ShowLineCommonMessage(ex.StackTrace + ex.Data + ex.Message);
                if (ex.InnerException != null)
                    Logger.ShowLineCommonMessage(ex.InnerException.StackTrace + ex.InnerException.Data + ex.InnerException.Message);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChannelsUsers>().HasKey(p => new { p.User_id, p.Channel_id });
            modelBuilder.Entity<Quotes>().HasKey(p => new { p.Channel_id, p.Number });
            modelBuilder.Entity<Counters>().HasKey(p => new { p.Channel_id, p.Number});
            modelBuilder.Entity<SubDayGames>().HasKey(p => new { p.Channel_id, p.Game_id });
           // modelBuilder.Entity<SubDayVotes>().HasKey(p => new { p.Game_id, p.Number});

        }

    }
}