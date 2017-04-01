using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using DudelkaBot.dataBase.model;

namespace DudelkaBot.Migrations
{
    [DbContext(typeof(ChatContext))]
    partial class ChatContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.1")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("DudelkaBot.dataBase.model.Channels", b =>
                {
                    b.Property<int>("Channel_id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Channel_name");

                    b.Property<int>("DeathCount");

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

                    b.Property<string>("Counter_name");

                    b.HasKey("Channel_id", "Number");

                    b.ToTable("Counters");
                });

            modelBuilder.Entity("DudelkaBot.dataBase.model.Quotes", b =>
                {
                    b.Property<int>("Channel_id");

                    b.Property<int>("Number");

                    b.Property<DateTime>("Date");

                    b.Property<string>("Quote");

                    b.HasKey("Channel_id", "Number");

                    b.ToTable("Quotes");
                });

            modelBuilder.Entity("DudelkaBot.dataBase.model.Users", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Username");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });
        }
    }
}
