namespace Cognexa.Application.Usuarios;
// Identidade autenticada é uma porta externa; o domínio continua gerando seus próprios IDs.
public interface IIdentidadeAtual
{
    Guid IdIdentidade
    {
        get;
    }
}
public interface IVinculoDeIdentidadeRepository
{
    Task<Guid?> ObterIdUsuarioAsync(Guid idIdentidade, CancellationToken cancellationToken);
    Task AssociarAsync(Guid idIdentidade, Guid idUsuario, CancellationToken cancellationToken);
}
