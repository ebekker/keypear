using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Keypear.Server.LocalServer.Migrations
{
    public partial class AddedDeletedDatetimes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDateTime",
                table: "Vaults",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDateTime",
                table: "Records",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedDateTime",
                table: "Vaults");

            migrationBuilder.DropColumn(
                name: "DeletedDateTime",
                table: "Records");
        }
    }
}
