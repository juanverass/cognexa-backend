using System.Security.Cryptography;
using System.Text.Json;
using Cognexa.Application.Anotacoes;
using Cognexa.Application.Compartilhado;
namespace Cognexa.Infrastructure.Inteligencia;

public sealed class OcrProvider(HttpClient http) : IOcrProvider
{
    public async Task<string> ExtrairAsync(byte[] imagem, string tipo, CancellationToken ct)
    {
        if (http.BaseAddress == null)
            throw new ServicoIndisponivelException("Provider OCR não configurado.");
        using var limiteTempo = CancellationTokenSource.CreateLinkedTokenSource(ct);
        limiteTempo.CancelAfter(TimeSpan.FromSeconds(30));
        var token = limiteTempo.Token;
        try
        {
            using var content = new ByteArrayContent(imagem);
            content.Headers.ContentType = new(tipo);
            using var request = new HttpRequestMessage(HttpMethod.Post, "extrair") { Content = content };
            using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
            if (!response.IsSuccessStatusCode)
                throw new ServicoIndisponivelException("Provider OCR indisponível.");
            await using var stream = await response.Content.ReadAsStreamAsync(token);
            using var buffer = new MemoryStream();
            var bytes = new byte[8192];
            try
            {
                int lidos;
                while ((lidos = await stream.ReadAsync(bytes, token)) > 0)
                {
                    if (buffer.Length + lidos > 500000)
                        throw new ServicoIndisponivelException("Resposta OCR excede o limite.");
                    await buffer.WriteAsync(bytes.AsMemory(0, lidos), token);
                }
                buffer.Position = 0;
                var result = await JsonSerializer.DeserializeAsync<Resposta>(buffer, new JsonSerializerOptions(JsonSerializerDefaults.Web), token);
                return result?.Texto ?? throw new ServicoIndisponivelException("Resposta OCR inválida.");
            }
            finally
            {
                CryptographicOperations.ZeroMemory(bytes);
                CryptographicOperations.ZeroMemory(buffer.GetBuffer());
            }
        }
        catch (Exception e) when (e is HttpRequestException or JsonException || (e is OperationCanceledException && !ct.IsCancellationRequested))
        {
            throw new ServicoIndisponivelException("Provider OCR indisponível.");
        }
    }
    private record Resposta(string Texto);
}
public sealed class OcrTemporario(TimeProvider tempo) : IOcrTemporario, IDisposable
{
    private readonly object trava = new();
    private readonly Dictionary<Guid, (Guid Usuario, string Texto, DateTimeOffset Expira)> capturas = [];
    private ITimer? limpeza;
    public Guid Guardar(Guid usuario, string texto)
    {
        lock (trava)
        {
            limpeza ??= tempo.CreateTimer(_ => Limpar(), null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
            Limpar();
            if (capturas.Count >= 100 || capturas.Values.Count(x => x.Usuario == usuario) >= 3)
                throw new Cognexa.Domain.Compartilhado.RegraDeDominioException("Limite de capturas temporárias atingido.");
            var id = Guid.NewGuid();
            capturas.Add(id, (usuario, texto, tempo.GetUtcNow().AddSeconds(120)));
            return id;
        }
    }
    private void Limpar()
    {
        lock (trava)
            foreach (var id in capturas.Where(x => x.Value.Expira <= tempo.GetUtcNow()).Select(x => x.Key).ToArray())
                capturas.Remove(id);
    }
    public string Consumir(Guid usuario, Guid id)
    {
        lock (trava)
        {
            Limpar();
            if (!capturas.TryGetValue(id, out var captura) || captura.Usuario != usuario)
                throw new NaoEncontradoException();
            capturas.Remove(id);
            return captura.Texto;
        }
    }
    public void Dispose()
    {
        limpeza?.Dispose();
        lock (trava)
            capturas.Clear();
    }
}
