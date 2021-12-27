using Microsoft.EntityFrameworkCore.Migrations;

namespace File_Sharing.Migrations
{
    public partial class addoriginalfilename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OriginalFileName",
                table: "Uploads",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                table: "Uploads");
        }
    }
}
