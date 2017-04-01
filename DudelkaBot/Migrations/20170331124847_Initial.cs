using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DudelkaBot.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Channels",
                columns: table => new
                {
                    Channel_id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Channel_name = table.Column<string>(nullable: true),
                    DeathCount = table.Column<int>(nullable: false),
                    VkId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Channels", x => x.Channel_id);
                });

            migrationBuilder.CreateTable(
                name: "ChannelsUsers",
                columns: table => new
                {
                    User_id = table.Column<int>(nullable: false),
                    Channel_id = table.Column<int>(nullable: false),
                    Active = table.Column<bool>(nullable: false),
                    CountMessage = table.Column<int>(nullable: false),
                    CountSubscriptions = table.Column<int>(nullable: false),
                    Moderator = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelsUsers", x => new { x.User_id, x.Channel_id });
                });

            migrationBuilder.CreateTable(
                name: "Counters",
                columns: table => new
                {
                    Channel_id = table.Column<int>(nullable: false),
                    Number = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Count = table.Column<int>(nullable: false),
                    Counter_name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Counters", x => new { x.Channel_id, x.Number });
                });

            migrationBuilder.CreateTable(
                name: "Quotes",
                columns: table => new
                {
                    Channel_id = table.Column<int>(nullable: false),
                    Number = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Quote = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotes", x => new { x.Channel_id, x.Number });
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Username = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Channels");

            migrationBuilder.DropTable(
                name: "ChannelsUsers");

            migrationBuilder.DropTable(
                name: "Counters");

            migrationBuilder.DropTable(
                name: "Quotes");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
