using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SequeMusic.Migrations
{
    /// <inheritdoc />
    public partial class AddResumoAndImagemUrlToNoticia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagemUrl",
                table: "Noticias",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Resumo",
                table: "Noticias",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagemUrl",
                table: "Noticias");

            migrationBuilder.DropColumn(
                name: "Resumo",
                table: "Noticias");
        }
    }
}
