using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cognexa.Infrastructure.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class Usuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    Preferencias_MetaDiariaEmMinutos = table.Column<int>(type: "integer", nullable: false),
                    Preferencias_Idioma = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VinculoDeIdentidade",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdIdentidade = table.Column<Guid>(type: "uuid", nullable: false),
                    IdUsuario = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VinculoDeIdentidade", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VinculoDeIdentidade_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VinculoDeIdentidade_IdIdentidade",
                table: "VinculoDeIdentidade",
                column: "IdIdentidade",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VinculoDeIdentidade_IdUsuario",
                table: "VinculoDeIdentidade",
                column: "IdUsuario",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VinculoDeIdentidade");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
