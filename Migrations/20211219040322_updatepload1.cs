using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace File_Sharing.Migrations
{
    public partial class updatepload1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UploadTime",
                table: "Uploads",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "GetDate()");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UploadTime",
                table: "Uploads");
        }
    }
}
