using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DudelkaBot.Migrations
{
    public partial class Initial2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_ChannelsUsers_User_id",
                table: "ChannelsUsers");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_ChannelsUsers_Channel_id_User_id",
                table: "ChannelsUsers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_ChannelsUsers_User_id",
                table: "ChannelsUsers",
                column: "User_id");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ChannelsUsers_Channel_id_User_id",
                table: "ChannelsUsers",
                columns: new[] { "Channel_id", "User_id" });
        }
    }
}
