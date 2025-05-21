using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SequeMusic.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarLetraAMusica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Letra",
                table: "Musicas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Letra",
                table: "Musicas");
        }
    }
}
