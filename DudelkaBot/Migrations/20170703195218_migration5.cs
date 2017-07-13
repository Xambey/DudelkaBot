using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DudelkaBot.Migrations
{
    public partial class migration5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeathCount",
                table: "Channels",
                newName: "DjId");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                nullable: false);

            migrationBuilder.AlterColumn<string>(
                name: "Quote",
                table: "Quotes",
                nullable: false);

            migrationBuilder.AlterColumn<string>(
                name: "Counter_name",
                table: "Counters",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Counters",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "SubDayGames",
                columns: table => new
                {
                    Channel_id = table.Column<int>(nullable: false),
                    Game_id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubDayGames", x => new { x.Channel_id, x.Game_id });
                });

            migrationBuilder.CreateTable(
                name: "SubDayVotes",
                columns: table => new
                {
                    Number = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Game_id = table.Column<int>(nullable: false),
                    UserName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubDayVotes", x => x.Number);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubDayGames");

            migrationBuilder.DropTable(
                name: "SubDayVotes");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Counters");

            migrationBuilder.RenameColumn(
                name: "DjId",
                table: "Channels",
                newName: "DeathCount");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Quote",
                table: "Quotes",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Counter_name",
                table: "Counters",
                nullable: true);
        }
    }
}
