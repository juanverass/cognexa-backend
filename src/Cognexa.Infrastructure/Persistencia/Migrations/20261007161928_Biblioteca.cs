using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cognexa.Infrastructure.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class Biblioteca : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Livro",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdUsuario = table.Column<Guid>(type: "uuid", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    TotalDePaginas = table.Column<int>(type: "integer", nullable: true),
                    Isbn = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Edicao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Capa = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Livro", x => x.Id);
                    table.UniqueConstraint("AK_Livro_Id_IdUsuario", x => new { x.Id, x.IdUsuario });
                    table.ForeignKey(
                        name: "FK_Livro_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Autor",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdLivro = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Autor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Autor_Livro_IdLivro",
                        column: x => x.IdLivro,
                        principalTable: "Livro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Capitulo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdUsuario = table.Column<Guid>(type: "uuid", nullable: false),
                    IdLivro = table.Column<Guid>(type: "uuid", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Ordem = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Capitulo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Capitulo_Livro_IdLivro_IdUsuario",
                        columns: x => new { x.IdLivro, x.IdUsuario },
                        principalTable: "Livro",
                        principalColumns: new[] { "Id", "IdUsuario" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Capitulo_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Leitura",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdUsuario = table.Column<Guid>(type: "uuid", nullable: false),
                    IdLivro = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PaginaAtual = table.Column<int>(type: "integer", nullable: false),
                    Motivo = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DataDeInicio = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DataDeConclusao = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leitura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Leitura_Livro_IdLivro_IdUsuario",
                        columns: x => new { x.IdLivro, x.IdUsuario },
                        principalTable: "Livro",
                        principalColumns: new[] { "Id", "IdUsuario" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Leitura_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Autor_IdLivro",
                table: "Autor",
                column: "IdLivro");

            migrationBuilder.CreateIndex(
                name: "IX_Capitulo_IdLivro_IdUsuario",
                table: "Capitulo",
                columns: new[] { "IdLivro", "IdUsuario" });

            migrationBuilder.CreateIndex(
                name: "IX_Capitulo_IdLivro_Ordem",
                table: "Capitulo",
                columns: new[] { "IdLivro", "Ordem" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Capitulo_IdUsuario",
                table: "Capitulo",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Leitura_IdLivro_IdUsuario",
                table: "Leitura",
                columns: new[] { "IdLivro", "IdUsuario" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Leitura_IdUsuario_Status",
                table: "Leitura",
                columns: new[] { "IdUsuario", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Livro_IdUsuario",
                table: "Livro",
                column: "IdUsuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Autor");

            migrationBuilder.DropTable(
                name: "Capitulo");

            migrationBuilder.DropTable(
                name: "Leitura");

            migrationBuilder.DropTable(
                name: "Livro");
        }
    }
}
