using Cognexa.Application.Usuarios;
namespace Cognexa.WebApi;

public static class UsuariosEndpoints
{
    public static void MapUsuarios(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/usuarios/me").RequireAuthorization();
        group.MapPost("", async (SalvarUsuarioDto dto, UsuarioAppService service, CancellationToken ct) => Results.Created("/usuarios/me", await service.CriarAsync(dto, ct)));
        group.MapGet("", (UsuarioAppService service, CancellationToken ct) => service.ObterAsync(ct));
        group.MapPut("", (SalvarUsuarioDto dto, UsuarioAppService service, CancellationToken ct) => service.AtualizarAsync(dto, ct));
        group.MapGet("/preferencias", async (UsuarioAppService service, CancellationToken ct) => (await service.ObterAsync(ct)).Preferencias);
        group.MapPut("/preferencias", (PreferenciasDoUsuarioDto dto, UsuarioAppService service, CancellationToken ct) => service.AtualizarPreferenciasAsync(dto, ct));
        group.MapDelete("", async (UsuarioAppService service, CancellationToken ct) => { await service.DesativarAsync(ct); return Results.NoContent(); });
    }
}
