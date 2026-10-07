using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cognexa.Infrastructure.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class SegurancaDeConteudo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Autor",
                table: "Anotacao",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MetodoDeCaptura",
                table: "Anotacao",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Moderacao",
                table: "Anotacao",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Obra",
                table: "Anotacao",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrigemDoComentario",
                table: "Anotacao",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Publicacao",
                table: "Anotacao",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Anotacao",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.CreateTable(
                name: "Denuncia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdAnotacao = table.Column<Guid>(type: "uuid", nullable: false),
                    IdDenunciante = table.Column<Guid>(type: "uuid", nullable: false),
                    Motivo = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Denuncia", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegistroDeConteudo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdUsuario = table.Column<Guid>(type: "uuid", nullable: false),
                    IdLivro = table.Column<Guid>(type: "uuid", nullable: false),
                    Caracteres = table.Column<int>(type: "integer", nullable: false),
                    Pagina = table.Column<int>(type: "integer", nullable: true),
                    Data = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistroDeConteudo", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Denuncia_IdAnotacao_IdDenunciante",
                table: "Denuncia",
                columns: new[] { "IdAnotacao", "IdDenunciante" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistroDeConteudo_IdLivro_Data",
                table: "RegistroDeConteudo",
                columns: new[] { "IdLivro", "Data" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Denuncia");

            migrationBuilder.DropTable(
                name: "RegistroDeConteudo");

            migrationBuilder.DropColumn(
                name: "Autor",
                table: "Anotacao");

            migrationBuilder.DropColumn(
                name: "MetodoDeCaptura",
                table: "Anotacao");

            migrationBuilder.DropColumn(
                name: "Moderacao",
                table: "Anotacao");

            migrationBuilder.DropColumn(
                name: "Obra",
                table: "Anotacao");

            migrationBuilder.DropColumn(
                name: "OrigemDoComentario",
                table: "Anotacao");

            migrationBuilder.DropColumn(
                name: "Publicacao",
                table: "Anotacao");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Anotacao");
        }
    }
}
