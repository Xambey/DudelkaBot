using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using DudelkaBot.dataBase.model;

namespace DudelkaBot.Migrations
{
    [DbContext(typeof(ChatContext))]
    [Migration("20170207181406_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.0-rtm-22752")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("DudelkaBot.dataBase.model.Channels", b =>
                {
                    b.Property<int>("Channel_id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Channel_name");

                    b.HasKey("Channel_id");

                    b.ToTable("Channels");
                });

            modelBuilder.Entity("DudelkaBot.dataBase.model.ChannelsUsers", b =>
                {
                    b.Property<int>("User_id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Channel_id");

                    b.Property<int>("CountMessage");

                    b.Property<int>("CountSubscriptions");

                    b.HasKey("User_id");

                    b.ToTable("ChannelsUsers");
                });

            modelBuilder.Entity("DudelkaBot.dataBase.model.Users", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("username");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });
        }
    }
}
