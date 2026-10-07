namespace Cognexa.Worker;
internal sealed class UsuarioDoWorker : Cognexa.Application.Compartilhado.IUsuarioAtual, Cognexa.Application.Usuarios.IIdentidadeAtual
{
    public UsuarioDoWorker() { }
    public Guid IdUsuario => throw new UnauthorizedAccessException("Um job deve definir seu contexto de usuário antes de executar casos de uso.");
    public Guid IdIdentidade => throw new UnauthorizedAccessException("Identidade autenticada indisponível em jobs.");
}
