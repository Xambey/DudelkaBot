using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using DudelkaBot.dataBase.model;

namespace DudelkaBot.Migrations
{
    [DbContext(typeof(ChatContext))]
    [Migration("20170210184932_Initial")]
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
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Channel_name");

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

                    b.HasAlternateKey("User_id");


                    b.HasAlternateKey("Channel_id", "User_id");

                    b.ToTable("ChannelsUsers");
                });

            modelBuilder.Entity("DudelkaBot.dataBase.model.Users", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Username");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });
        }
    }
}
