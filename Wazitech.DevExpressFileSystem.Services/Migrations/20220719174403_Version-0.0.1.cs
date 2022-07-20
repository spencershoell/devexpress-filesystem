using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wazitech.DevExpressFileSystem.Services.Migrations
{
    public partial class Version001 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Content",
                table: "FileItems",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "FileItems");
        }
    }
}
