using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MerkatorS.Migrations
{
    /// <inheritdoc />
    public partial class tryy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AwayTeamName",
                table: "Matches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HomeTeamName",
                table: "Matches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AwayTeamName",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "HomeTeamName",
                table: "Matches");
        }
    }
}
