using System.Security.Cryptography;
using Cognexa.Application.Compartilhado;
using Cognexa.Domain.Anotacoes;
using Cognexa.Domain.Compartilhado;
namespace Cognexa.Application.Anotacoes;
public interface IOcrProvider { Task<string> ExtrairAsync(byte[] imagem, string tipo, CancellationToken ct); }
public interface IOcrTemporario
{
    Guid Guardar(Guid usuario, string texto);
    string Consumir(Guid usuario, Guid id);
}
public record ResultadoOcrDto(Guid IdCaptura, string Texto, int ExpiraEmSegundos);
public record ConfirmarOcrDto(Guid IdCaptura, int Inicio, int Comprimento, Guid IdLivro, int? Pagina, string? Localizacao, string? Comentario);
public sealed class OcrAppService(IOcrProvider provider, IOcrTemporario temporario, IUsuarioAtual atual, AnotacaoAppService anotacoes)
{
    public async Task<ResultadoOcrDto> ExtrairAsync(byte[] imagem, string tipo, CancellationToken ct)
    {
        try
        {
            if (imagem.Length is < 8 or > 5000000 || tipo is not ("image/png" or "image/jpeg")) throw new RegraDeDominioException("Imagem inválida (PNG/JPEG, até 5 MB).");
            var png = imagem.AsSpan(0, 8).SequenceEqual(new byte[] {137, 80, 78, 71, 13, 10, 26, 10});
            var jpeg = imagem[0] == 255 && imagem[1] == 216 && imagem[2] == 255;
            if ((tipo == "image/png" && !png) || (tipo == "image/jpeg" && !jpeg)) throw new RegraDeDominioException("Formato da imagem inválido.");
            var texto = await provider.ExtrairAsync(imagem, tipo, ct);
            if (string.IsNullOrWhiteSpace(texto) || texto.Length > 100000) throw new ServicoIndisponivelException("Resposta OCR inválida.");
            return new(temporario.Guardar(atual.IdUsuario, texto), texto, 120);
        }
        finally { CryptographicOperations.ZeroMemory(imagem); }
    }
    public Task<AnotacaoDto> ConfirmarAsync(ConfirmarOcrDto dto, CancellationToken ct)
    {
        var texto = temporario.Consumir(atual.IdUsuario, dto.IdCaptura);
        if (dto.Inicio < 0 || dto.Comprimento < 1 || dto.Comprimento > texto.Length || dto.Inicio > texto.Length - dto.Comprimento) throw new RegraDeDominioException("Seleção inválida.");
        return anotacoes.CriarAsync(new(dto.IdLivro, null, TipoDeAnotacao.Citacao, texto.Substring(dto.Inicio, dto.Comprimento), dto.Comentario, dto.Pagina, dto.Localizacao, MetodoDeCaptura.Ocr), ct);
    }
}
