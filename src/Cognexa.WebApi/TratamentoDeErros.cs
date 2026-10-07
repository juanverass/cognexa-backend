using Cognexa.Application.Compartilhado;
using Cognexa.Domain.Compartilhado;
using Microsoft.AspNetCore.Diagnostics;
namespace Cognexa.WebApi;

public sealed class TratamentoDeErros(IProblemDetailsService problems, ILogger<TratamentoDeErros> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            RegraDeDominioException => (400, "Dados inválidos"),
            BadHttpRequestException => (400, "Requisição inválida"),
            NaoEncontradoException => (404, "Recurso não encontrado"),
            ConflitoException => (409, "Conflito"),
            ServicoIndisponivelException => (503, "Serviço indisponível"),
            UnauthorizedAccessException => (401, "Autenticação necessária"),
            _ => (500, "Erro interno")
        };
        if (status >= 500)
            logger.LogError(exception, "Falha na requisição {TraceId}", context.TraceIdentifier);
        context.Response.StatusCode = status;
        return await problems.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = new() { Status = status, Title = title, Detail = status is 400 or 409 ? exception.Message : null, Extensions = { ["traceId"] = context.TraceIdentifier } }
        });
    }
}
