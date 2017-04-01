using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DudelkaBot.dataBase.model
{
    public class ChatContext : DbContext, IDisposable
    {
        public DbSet<Users> Users { get; set; }
        public DbSet<Channels> Channels { get; set; }
        public DbSet<ChannelsUsers> ChannelsUsers { get; set; }
        public DbSet<Quotes> Quotes { get; set; }
        public DbSet<Counters> Counters { get; set; }

        public ChatContext() : base() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=DudelkaBotBase;Trusted_Connection=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChannelsUsers>().HasKey(p => new { p.User_id, p.Channel_id });
            modelBuilder.Entity<Quotes>().HasKey(p => new { p.Channel_id, p.Number });
            modelBuilder.Entity<Counters>().HasKey(p => new { p.Channel_id, p.Number});
        }
    }
}