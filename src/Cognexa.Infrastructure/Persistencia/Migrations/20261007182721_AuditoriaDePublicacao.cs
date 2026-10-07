using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cognexa.Infrastructure.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class AuditoriaDePublicacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegistroDePublicacao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdAnotacao = table.Column<Guid>(type: "uuid", nullable: false),
                    IdLivro = table.Column<Guid>(type: "uuid", nullable: false),
                    IdAtor = table.Column<Guid>(type: "uuid", nullable: false),
                    Obra = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Autor = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Publicacao = table.Column<int>(type: "integer", nullable: false),
                    Moderacao = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistroDePublicacao", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegistroDePublicacao_IdAnotacao_Data",
                table: "RegistroDePublicacao",
                columns: new[] { "IdAnotacao", "Data" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistroDePublicacao");
        }
    }
}
