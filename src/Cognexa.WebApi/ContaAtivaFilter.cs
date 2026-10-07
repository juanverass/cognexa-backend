using Cognexa.Application.Usuarios;
namespace Cognexa.WebApi;

public sealed class ContaAtivaFilter(UsuarioAppService usuarios) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        await usuarios.ObterAtivoAsync(context.HttpContext.RequestAborted);
        return await next(context);
    }
}
