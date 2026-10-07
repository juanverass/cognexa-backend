using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Cognexa.Application.Compartilhado;
using Cognexa.Application.Usuarios;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
namespace Cognexa.WebApi;
// Adapter inicial para tokens opacos provisionados por um serviço de identidade externo.
// A configuração é externa; nenhum token ou provider faz parte do domínio.
public sealed class AutenticacaoOptions : AuthenticationSchemeOptions
{
    public List<IdentidadeConfigurada> Identidades { get; set; } = [];
}
public sealed class IdentidadeConfigurada
{
    public Guid IdIdentidade
    {
        get; set;
    }
    public string Token { get; set; } = "";
}
public sealed class AutenticacaoHandler(IOptionsMonitor<AutenticacaoOptions> options, ILoggerFactory logger, UrlEncoder encoder, IVinculoDeIdentidadeRepository vinculos)
    : AuthenticationHandler<AutenticacaoOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var header = Request.Headers.Authorization.ToString();
        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.NoResult();
        var token = header[7..];
        if (token.Length is < 32 or > 512)
            return AuthenticateResult.Fail("Token inválido.");
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        var identidades = Options.Identidades.Where(x => x.IdIdentidade != Guid.Empty && x.Token is { Length: >= 32 and <= 512 }
            && CryptographicOperations.FixedTimeEquals(hash, SHA256.HashData(Encoding.UTF8.GetBytes(x.Token)))).ToArray();
        if (identidades.Length != 1)
            return AuthenticateResult.Fail("Token inválido.");
        var identidade = identidades[0];
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, identidade.IdIdentidade.ToString()) };
        var idUsuario = await vinculos.ObterIdUsuarioAsync(identidade.IdIdentidade, Context.RequestAborted);
        if (idUsuario.HasValue)
            claims.Add(new("id_usuario", idUsuario.Value.ToString()));
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
        return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
    }
}
public sealed class UsuarioAtual(IHttpContextAccessor accessor) : IUsuarioAtual, IIdentidadeAtual
{
    public Guid IdUsuario => Guid.TryParse(accessor.HttpContext?.User.FindFirstValue("id_usuario"), out var id) && id != Guid.Empty
        ? id : throw new UnauthorizedAccessException();
    public Guid IdIdentidade => Guid.TryParse(accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id != Guid.Empty
        ? id : throw new UnauthorizedAccessException();
}
