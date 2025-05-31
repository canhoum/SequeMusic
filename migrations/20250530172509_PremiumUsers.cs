using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SequeMusic.Migrations
{
    /// <inheritdoc />
    public partial class PremiumUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPremium",
                table: "Musicas");

            migrationBuilder.AddColumn<bool>(
                name: "IsPremium",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPremium",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<bool>(
                name: "IsPremium",
                table: "Musicas",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
