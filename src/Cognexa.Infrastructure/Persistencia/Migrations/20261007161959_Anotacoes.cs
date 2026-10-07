using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cognexa.Infrastructure.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class Anotacoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_Capitulo_Id_IdLivro_IdUsuario",
                table: "Capitulo",
                columns: new[] { "Id", "IdLivro", "IdUsuario" });

            migrationBuilder.CreateTable(
                name: "Anotacao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdUsuario = table.Column<Guid>(type: "uuid", nullable: false),
                    IdLivro = table.Column<Guid>(type: "uuid", nullable: false),
                    IdCapitulo = table.Column<Guid>(type: "uuid", nullable: true),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    TrechoOriginal = table.Column<string>(type: "character varying(20000)", maxLength: 20000, nullable: true),
                    Comentario = table.Column<string>(type: "character varying(20000)", maxLength: 20000, nullable: true),
                    Pagina = table.Column<int>(type: "integer", nullable: true),
                    Localizacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DataDeCriacao = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anotacao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Anotacao_Capitulo_IdCapitulo_IdLivro_IdUsuario",
                        columns: x => new { x.IdCapitulo, x.IdLivro, x.IdUsuario },
                        principalTable: "Capitulo",
                        principalColumns: new[] { "Id", "IdLivro", "IdUsuario" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Anotacao_Livro_IdLivro_IdUsuario",
                        columns: x => new { x.IdLivro, x.IdUsuario },
                        principalTable: "Livro",
                        principalColumns: new[] { "Id", "IdUsuario" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Anotacao_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Anotacao_IdCapitulo_IdLivro_IdUsuario",
                table: "Anotacao",
                columns: new[] { "IdCapitulo", "IdLivro", "IdUsuario" });

            migrationBuilder.CreateIndex(
                name: "IX_Anotacao_IdLivro_IdUsuario",
                table: "Anotacao",
                columns: new[] { "IdLivro", "IdUsuario" });

            migrationBuilder.CreateIndex(
                name: "IX_Anotacao_IdUsuario_IdLivro_Tipo_Pagina",
                table: "Anotacao",
                columns: new[] { "IdUsuario", "IdLivro", "Tipo", "Pagina" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Anotacao");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Capitulo_Id_IdLivro_IdUsuario",
                table: "Capitulo");
        }
    }
}
