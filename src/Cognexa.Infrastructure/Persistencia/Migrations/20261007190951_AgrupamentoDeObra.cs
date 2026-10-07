using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cognexa.Infrastructure.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class AgrupamentoDeObra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RegistroDeConteudo_IdLivro_Data",
                table: "RegistroDeConteudo");

            migrationBuilder.AddColumn<string>(
                name: "ChaveDaObra",
                table: "RegistroDeConteudo",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ChaveDaObra",
                table: "Livro",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                CREATE FUNCTION pg_temp.cognexa_normalizar_obra(text) RETURNS text
                LANGUAGE SQL IMMUTABLE STRICT AS $$
                    SELECT COALESCE(string_agg(lower(caractere COLLATE "und-x-icu"), '' ORDER BY ordem), '')
                    FROM regexp_split_to_table(regexp_replace(normalize($1, NFD) COLLATE "und-x-icu", '[^[:alnum:]]', '', 'g'), '')
                        WITH ORDINALITY AS caracteres(caractere, ordem)
                $$;
                UPDATE "Livro" AS livro
                SET "ChaveDaObra" = encode(sha256(convert_to(
                    'obra:v1|' || pg_temp.cognexa_normalizar_obra(livro."Titulo") || '|' ||
                    COALESCE((SELECT string_agg(autores.nome, '|' ORDER BY autores.nome COLLATE "C")
                        FROM (SELECT DISTINCT pg_temp.cognexa_normalizar_obra(a."Nome") AS nome
                              FROM "Autor" a WHERE a."IdLivro" = livro."Id") autores), ''),
                    'UTF8')), 'hex');
                UPDATE "RegistroDeConteudo" AS registro
                SET "ChaveDaObra" = livro."ChaveDaObra"
                FROM "Livro" AS livro WHERE livro."Id" = registro."IdLivro";
                UPDATE "RegistroDeConteudo"
                SET "ChaveDaObra" = repeat('0', 64) WHERE "ChaveDaObra" = '';
                DROP FUNCTION pg_temp.cognexa_normalizar_obra(text);
                """);

            migrationBuilder.CreateIndex(
                name: "IX_RegistroDeConteudo_IdUsuario_ChaveDaObra_Data",
                table: "RegistroDeConteudo",
                columns: new[] { "IdUsuario", "ChaveDaObra", "Data" });

            migrationBuilder.CreateIndex(
                name: "IX_Livro_ChaveDaObra",
                table: "Livro",
                column: "ChaveDaObra");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RegistroDeConteudo_IdUsuario_ChaveDaObra_Data",
                table: "RegistroDeConteudo");

            migrationBuilder.DropIndex(
                name: "IX_Livro_ChaveDaObra",
                table: "Livro");

            migrationBuilder.DropColumn(
                name: "ChaveDaObra",
                table: "RegistroDeConteudo");

            migrationBuilder.DropColumn(
                name: "ChaveDaObra",
                table: "Livro");

            migrationBuilder.CreateIndex(
                name: "IX_RegistroDeConteudo_IdLivro_Data",
                table: "RegistroDeConteudo",
                columns: new[] { "IdLivro", "Data" });
        }
    }
}
