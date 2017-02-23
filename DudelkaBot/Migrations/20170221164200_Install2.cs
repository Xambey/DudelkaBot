using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DudelkaBot.Migrations
{
    public partial class Install2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeathCount",
                table: "Channels",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VkId",
                table: "Channels",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeathCount",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "VkId",
                table: "Channels");
        }
    }
}
