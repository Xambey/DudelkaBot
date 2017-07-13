using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using DudelkaBot.dataBase.model;

namespace DudelkaBot.Migrations
{
    [DbContext(typeof(ChatContext))]
    [Migration("20170703195218_migration5")]
    partial class migration5
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.1")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("DudelkaBot.dataBase.model.Channels", b =>
                {
                    b.Property<int>("Channel_id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Channel_name");

                    b.Property<int>("DjId");

                    b.Property<int>("VkId");

                    b.HasKey("Channel_id");

                    b.ToTable("Channels");
                });

            modelBuilder.Entity("DudelkaBot.dataBase.model.ChannelsUsers", b =>
                {
                    b.Property<int>("User_id");

                    b.Property<int>("Channel_id");

                    b.Property<bool>("Active");

                    b.Property<int>("CountMessage");

                    b.Property<int>("CountSubscriptions");

                    b.Property<bool>("Moderator");

                    b.HasKey("User_id", "Channel_id");

                    b.ToTable("ChannelsUsers");
                });

            modelBuilder.Entity("DudelkaBot.dataBase.model.Counters", b =>
                {
                    b.Property<int>("Channel_id");

                    b.Property<int>("Number")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Count");

                    b.Property<string>("Counter_name")
                        .IsRequired();

                    b.Property<string>("Description")
                        .IsRequired();

                    b.HasKey("Channel_id", "Number");

                    b.ToTable("Counters");
                });

            modelBuilder.Entity("DudelkaBot.dataBase.model.Quotes", b =>
                {
                    b.Property<int>("Channel_id");

                    b.Property<int>("Number");

                    b.Property<DateTime>("Date");

                    b.Property<string>("Quote")
                        .IsRequired();

                    b.HasKey("Channel_id", "Number");

                    b.ToTable("Quotes");
                });

            modelBuilder.Entity("DudelkaBot.dataBase.model.SubDayGames", b =>
                {
                    b.Property<int>("Channel_id");

                    b.Property<int>("Game_id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<int>("Value");

                    b.HasKey("Channel_id", "Game_id");

                    b.ToTable("SubDayGames");
                });

            modelBuilder.Entity("DudelkaBot.dataBase.model.SubDayVotes", b =>
                {
                    b.Property<int>("Number")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Game_id");

                    b.Property<string>("UserName")
                        .IsRequired();

                    b.HasKey("Number");

                    b.ToTable("SubDayVotes");
                });

            modelBuilder.Entity("DudelkaBot.dataBase.model.Users", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Username")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("Users");
                });
        }
    }
}
