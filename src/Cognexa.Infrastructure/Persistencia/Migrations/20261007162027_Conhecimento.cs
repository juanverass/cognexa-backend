using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cognexa.Infrastructure.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class Conhecimento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_Anotacao_Id_IdLivro_IdUsuario",
                table: "Anotacao",
                columns: new[] { "Id", "IdLivro", "IdUsuario" });

            migrationBuilder.CreateTable(
                name: "Aprendizado",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdUsuario = table.Column<Guid>(type: "uuid", nullable: false),
                    Origem = table.Column<int>(type: "integer", nullable: false),
                    Conteudo = table.Column<string>(type: "character varying(20000)", maxLength: 20000, nullable: false),
                    DataDeCriacao = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aprendizado", x => x.Id);
                    table.UniqueConstraint("AK_Aprendizado_Id_IdUsuario", x => new { x.Id, x.IdUsuario });
                    table.ForeignKey(
                        name: "FK_Aprendizado_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Conceito",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdUsuario = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conceito", x => x.Id);
                    table.UniqueConstraint("AK_Conceito_Id_IdUsuario", x => new { x.Id, x.IdUsuario });
                    table.ForeignKey(
                        name: "FK_Conceito_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AplicacaoPratica",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdUsuario = table.Column<Guid>(type: "uuid", nullable: false),
                    IdAprendizado = table.Column<Guid>(type: "uuid", nullable: false),
                    Origem = table.Column<int>(type: "integer", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(20000)", maxLength: 20000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AplicacaoPratica", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AplicacaoPratica_Aprendizado_IdAprendizado_IdUsuario",
                        columns: x => new { x.IdAprendizado, x.IdUsuario },
                        principalTable: "Aprendizado",
                        principalColumns: new[] { "Id", "IdUsuario" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AplicacaoPratica_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FonteDoAprendizado",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdUsuario = table.Column<Guid>(type: "uuid", nullable: false),
                    IdAprendizado = table.Column<Guid>(type: "uuid", nullable: false),
                    IdAnotacao = table.Column<Guid>(type: "uuid", nullable: false),
                    IdLivro = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FonteDoAprendizado", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FonteDoAprendizado_Anotacao_IdAnotacao_IdLivro_IdUsuario",
                        columns: x => new { x.IdAnotacao, x.IdLivro, x.IdUsuario },
                        principalTable: "Anotacao",
                        principalColumns: new[] { "Id", "IdLivro", "IdUsuario" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FonteDoAprendizado_Aprendizado_IdAprendizado_IdUsuario",
                        columns: x => new { x.IdAprendizado, x.IdUsuario },
                        principalTable: "Aprendizado",
                        principalColumns: new[] { "Id", "IdUsuario" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FonteDoAprendizado_Livro_IdLivro_IdUsuario",
                        columns: x => new { x.IdLivro, x.IdUsuario },
                        principalTable: "Livro",
                        principalColumns: new[] { "Id", "IdUsuario" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConceitoDoAprendizado",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdUsuario = table.Column<Guid>(type: "uuid", nullable: false),
                    IdAprendizado = table.Column<Guid>(type: "uuid", nullable: false),
                    IdConceito = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConceitoDoAprendizado", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConceitoDoAprendizado_Aprendizado_IdAprendizado_IdUsuario",
                        columns: x => new { x.IdAprendizado, x.IdUsuario },
                        principalTable: "Aprendizado",
                        principalColumns: new[] { "Id", "IdUsuario" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConceitoDoAprendizado_Conceito_IdConceito_IdUsuario",
                        columns: x => new { x.IdConceito, x.IdUsuario },
                        principalTable: "Conceito",
                        principalColumns: new[] { "Id", "IdUsuario" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RelacaoEntreConceitos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdUsuario = table.Column<Guid>(type: "uuid", nullable: false),
                    IdConceitoOrigem = table.Column<Guid>(type: "uuid", nullable: false),
                    IdConceitoDestino = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelacaoEntreConceitos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RelacaoEntreConceitos_Conceito_IdConceitoDestino_IdUsuario",
                        columns: x => new { x.IdConceitoDestino, x.IdUsuario },
                        principalTable: "Conceito",
                        principalColumns: new[] { "Id", "IdUsuario" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RelacaoEntreConceitos_Conceito_IdConceitoOrigem_IdUsuario",
                        columns: x => new { x.IdConceitoOrigem, x.IdUsuario },
                        principalTable: "Conceito",
                        principalColumns: new[] { "Id", "IdUsuario" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RelacaoEntreConceitos_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AplicacaoPratica_IdAprendizado_IdUsuario",
                table: "AplicacaoPratica",
                columns: new[] { "IdAprendizado", "IdUsuario" });

            migrationBuilder.CreateIndex(
                name: "IX_AplicacaoPratica_IdUsuario_IdAprendizado",
                table: "AplicacaoPratica",
                columns: new[] { "IdUsuario", "IdAprendizado" });

            migrationBuilder.CreateIndex(
                name: "IX_Aprendizado_IdUsuario",
                table: "Aprendizado",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Conceito_IdUsuario_Nome",
                table: "Conceito",
                columns: new[] { "IdUsuario", "Nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConceitoDoAprendizado_IdAprendizado_IdConceito",
                table: "ConceitoDoAprendizado",
                columns: new[] { "IdAprendizado", "IdConceito" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConceitoDoAprendizado_IdAprendizado_IdUsuario",
                table: "ConceitoDoAprendizado",
                columns: new[] { "IdAprendizado", "IdUsuario" });

            migrationBuilder.CreateIndex(
                name: "IX_ConceitoDoAprendizado_IdConceito_IdUsuario",
                table: "ConceitoDoAprendizado",
                columns: new[] { "IdConceito", "IdUsuario" });

            migrationBuilder.CreateIndex(
                name: "IX_FonteDoAprendizado_IdAnotacao_IdLivro_IdUsuario",
                table: "FonteDoAprendizado",
                columns: new[] { "IdAnotacao", "IdLivro", "IdUsuario" });

            migrationBuilder.CreateIndex(
                name: "IX_FonteDoAprendizado_IdAprendizado_IdAnotacao",
                table: "FonteDoAprendizado",
                columns: new[] { "IdAprendizado", "IdAnotacao" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FonteDoAprendizado_IdAprendizado_IdUsuario",
                table: "FonteDoAprendizado",
                columns: new[] { "IdAprendizado", "IdUsuario" });

            migrationBuilder.CreateIndex(
                name: "IX_FonteDoAprendizado_IdLivro_IdUsuario",
                table: "FonteDoAprendizado",
                columns: new[] { "IdLivro", "IdUsuario" });

            migrationBuilder.CreateIndex(
                name: "IX_RelacaoEntreConceitos_IdConceitoDestino_IdUsuario",
                table: "RelacaoEntreConceitos",
                columns: new[] { "IdConceitoDestino", "IdUsuario" });

            migrationBuilder.CreateIndex(
                name: "IX_RelacaoEntreConceitos_IdConceitoOrigem_IdUsuario",
                table: "RelacaoEntreConceitos",
                columns: new[] { "IdConceitoOrigem", "IdUsuario" });

            migrationBuilder.CreateIndex(
                name: "IX_RelacaoEntreConceitos_IdUsuario_IdConceitoOrigem_IdConceito~",
                table: "RelacaoEntreConceitos",
                columns: new[] { "IdUsuario", "IdConceitoOrigem", "IdConceitoDestino", "Tipo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AplicacaoPratica");

            migrationBuilder.DropTable(
                name: "ConceitoDoAprendizado");

            migrationBuilder.DropTable(
                name: "FonteDoAprendizado");

            migrationBuilder.DropTable(
                name: "RelacaoEntreConceitos");

            migrationBuilder.DropTable(
                name: "Aprendizado");

            migrationBuilder.DropTable(
                name: "Conceito");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Anotacao_Id_IdLivro_IdUsuario",
                table: "Anotacao");
        }
    }
}
