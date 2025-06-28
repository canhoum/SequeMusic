using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SequeMusic.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarRelacaoNnArtistaMusicaCorrigida : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArtistasMusicas",
                columns: table => new
                {
                    ArtistaId = table.Column<int>(type: "int", nullable: false),
                    MusicaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistasMusicas", x => new { x.ArtistaId, x.MusicaId });
                    table.ForeignKey(
                        name: "FK_ArtistasMusicas_Artistas_ArtistaId",
                        column: x => x.ArtistaId,
                        principalTable: "Artistas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArtistasMusicas_Musicas_MusicaId",
                        column: x => x.MusicaId,
                        principalTable: "Musicas",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArtistasMusicas_MusicaId",
                table: "ArtistasMusicas",
                column: "MusicaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArtistasMusicas");
        }
    }
}
