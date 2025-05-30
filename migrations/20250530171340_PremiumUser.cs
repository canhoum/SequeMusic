using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SequeMusic.Migrations
{
    /// <inheritdoc />
    public partial class PremiumUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPremium",
                table: "Musicas",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPremium",
                table: "Musicas");
        }
    }
}
