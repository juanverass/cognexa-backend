using System.Security.Cryptography;
using System.Text;
namespace Cognexa.Domain.Biblioteca;

public static class ChaveDeObra
{
    // Históricos antigos sem livro sobrevivente não permitem recuperar a atribuição.
    public const string LegadoIndeterminado = "0000000000000000000000000000000000000000000000000000000000000000";
    public static string Criar(string titulo, IEnumerable<string> autores)
    {
        static string Normalizar(string texto) => string.Concat(texto.Normalize(NormalizationForm.FormD).EnumerateRunes()
            .Where(Rune.IsLetterOrDigit).Select(rune => Rune.ToLowerInvariant(rune).ToString()));
        var nomes = autores.Select(Normalizar).Distinct(StringComparer.Ordinal).OrderBy(nome => Convert.ToHexString(Encoding.UTF8.GetBytes(nome)), StringComparer.Ordinal);
        var identidade = "obra:v1|" + Normalizar(titulo) + "|" + string.Join("|", nomes);
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(identidade)));
    }
}
