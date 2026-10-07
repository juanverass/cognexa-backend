using Cognexa.Application.Inteligencia;
namespace Cognexa.WebApi;

public static class InteligenciaEndpoints
{
    public static void MapInteligencia(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/inteligencia").RequireAuthorization().AddEndpointFilter<ContaAtivaFilter>();
        group.MapPost("/sugestoes", (PedidoInteligenciaDto dto, InteligenciaAppService service, CancellationToken ct) => service.SugerirAsync(dto, ct));
        group.MapGet("/busca", (string consulta, int? limite, InteligenciaAppService service, CancellationToken ct) => service.BuscarAsync(consulta, limite ?? 10, ct));
    }
}
