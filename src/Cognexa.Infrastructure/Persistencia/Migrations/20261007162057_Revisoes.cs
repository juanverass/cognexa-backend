using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cognexa.Infrastructure.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class Revisoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PerguntaDeRevisao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdUsuario = table.Column<Guid>(type: "uuid", nullable: false),
                    IdAprendizado = table.Column<Guid>(type: "uuid", nullable: false),
                    Origem = table.Column<int>(type: "integer", nullable: false),
                    Pergunta = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    RespostaEsperada = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerguntaDeRevisao", x => x.Id);
                    table.UniqueConstraint("AK_PerguntaDeRevisao_Id_IdUsuario", x => new { x.Id, x.IdUsuario });
                    table.ForeignKey(
                        name: "FK_PerguntaDeRevisao_Aprendizado_IdAprendizado_IdUsuario",
                        columns: x => new { x.IdAprendizado, x.IdUsuario },
                        principalTable: "Aprendizado",
                        principalColumns: new[] { "Id", "IdUsuario" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PerguntaDeRevisao_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Revisao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdUsuario = table.Column<Guid>(type: "uuid", nullable: false),
                    IdPergunta = table.Column<Guid>(type: "uuid", nullable: false),
                    ProximaRevisao = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UltimaRevisao = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AcertosConsecutivos = table.Column<int>(type: "integer", nullable: false),
                    NivelDeDominio = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Revisao", x => x.Id);
                    table.UniqueConstraint("AK_Revisao_Id_IdUsuario", x => new { x.Id, x.IdUsuario });
                    table.ForeignKey(
                        name: "FK_Revisao_PerguntaDeRevisao_IdPergunta_IdUsuario",
                        columns: x => new { x.IdPergunta, x.IdUsuario },
                        principalTable: "PerguntaDeRevisao",
                        principalColumns: new[] { "Id", "IdUsuario" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Revisao_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoDeRevisao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdUsuario = table.Column<Guid>(type: "uuid", nullable: false),
                    IdRevisao = table.Column<Guid>(type: "uuid", nullable: false),
                    Resultado = table.Column<int>(type: "integer", nullable: false),
                    Resposta = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: true),
                    Data = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProximaRevisao = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    NivelAnterior = table.Column<int>(type: "integer", nullable: false),
                    NivelNovo = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoDeRevisao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoricoDeRevisao_Revisao_IdRevisao_IdUsuario",
                        columns: x => new { x.IdRevisao, x.IdUsuario },
                        principalTable: "Revisao",
                        principalColumns: new[] { "Id", "IdUsuario" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistoricoDeRevisao_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoDeRevisao_IdRevisao_IdUsuario",
                table: "HistoricoDeRevisao",
                columns: new[] { "IdRevisao", "IdUsuario" });

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoDeRevisao_IdUsuario_IdRevisao_Data",
                table: "HistoricoDeRevisao",
                columns: new[] { "IdUsuario", "IdRevisao", "Data" });

            migrationBuilder.CreateIndex(
                name: "IX_PerguntaDeRevisao_IdAprendizado_IdUsuario",
                table: "PerguntaDeRevisao",
                columns: new[] { "IdAprendizado", "IdUsuario" });

            migrationBuilder.CreateIndex(
                name: "IX_PerguntaDeRevisao_IdUsuario_IdAprendizado",
                table: "PerguntaDeRevisao",
                columns: new[] { "IdUsuario", "IdAprendizado" });

            migrationBuilder.CreateIndex(
                name: "IX_Revisao_IdPergunta_IdUsuario",
                table: "Revisao",
                columns: new[] { "IdPergunta", "IdUsuario" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Revisao_IdUsuario_ProximaRevisao",
                table: "Revisao",
                columns: new[] { "IdUsuario", "ProximaRevisao" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoricoDeRevisao");

            migrationBuilder.DropTable(
                name: "Revisao");

            migrationBuilder.DropTable(
                name: "PerguntaDeRevisao");
        }
    }
}
