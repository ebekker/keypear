using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Keypear.Server.LocalServer.Migrations
{
    public partial class RemovedVaultFastAndFullContent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FastContentEnc",
                table: "Vaults");

            migrationBuilder.DropColumn(
                name: "FullContentEnc",
                table: "Vaults");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "FastContentEnc",
                table: "Vaults",
                type: "BLOB",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "FullContentEnc",
                table: "Vaults",
                type: "BLOB",
                nullable: true);
        }
    }
}
