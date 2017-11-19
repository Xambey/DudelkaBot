﻿using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;
using System.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DudelkaBot.Logging;
using MySql.Data.MySqlClient;

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
        public DbSet<Gamers> Gamers { get; set; }
        public MySqlConnection con;

        public ChatContext() : base() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            try
            {
                //optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=DudelkaBotBase;Trusted_Connection=True;");
                optionsBuilder.UseMySql(@"Server=localhost;Database=DudelkaBotBase;Uid=xambey;Pwd=Passw0rd;");
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

        protected override void OnModelCreating(ModelBuilder modelbuilder)
        {
            modelbuilder.Entity<ChannelsUsers>().HasKey(p => new { p.User_id, p.Channel_id});
            modelbuilder.Entity<Quotes>().HasKey(p => new { p.Channel_id, p.Number});
            modelbuilder.Entity<Counters>().HasKey(p => new { p.Channel_id, p.Number });
            modelbuilder.Entity<SubDayGames>().HasKey(p => new { p.Channel_id, p.Game_id});
            modelbuilder.Entity<Gamers>().HasKey(p => new { p.Channel_ID, p.User_ID });
            //modelbuilder.Entity<SubDayVotes>().HasKey(p => new { p.Game_id, p.Number});

        }

        public int SaveChanges<TEntity>() where TEntity : class
        {
            var original = this.ChangeTracker.Entries()
                .Where(x => !typeof(TEntity).IsAssignableFrom(x.Entity.GetType()) && x.State != EntityState.Unchanged)
                .GroupBy(x => x.State)
                .ToList();

            foreach (var entry in this.ChangeTracker.Entries().Where(x => !typeof(TEntity).IsAssignableFrom(x.Entity.GetType())))
            {
                entry.State = EntityState.Unchanged;
            }

            var rows = base.SaveChanges();

            foreach (var state in original)
            {
                foreach (var entry in state)
                {
                    entry.State = state.Key;
                }
            }

            return rows;
        }

    }
}
