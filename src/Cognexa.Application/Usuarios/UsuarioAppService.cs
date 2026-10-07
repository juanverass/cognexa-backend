using Cognexa.Application.Compartilhado;
using Cognexa.Domain.Compartilhado;
using Cognexa.Domain.Usuarios;
using Mapster;
namespace Cognexa.Application.Usuarios;

public record PreferenciasDoUsuarioDto(int MetaDiariaEmMinutos, string Idioma);
public record UsuarioDto(Guid Id, string Nome, bool Ativo, PreferenciasDoUsuarioDto Preferencias);
public record SalvarUsuarioDto(string Nome);
public sealed class MapeamentoUsuario : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Usuario, UsuarioDto>();
        config.NewConfig<PreferenciasDoUsuario, PreferenciasDoUsuarioDto>();
    }
}
public sealed class UsuarioAppService(IRepository<Usuario> repository, IUnitOfWork unitOfWork, IUsuarioAtual atual, IIdentidadeAtual identidade, IVinculoDeIdentidadeRepository vinculos, TypeAdapterConfig config)
{
    public async Task<UsuarioDto> CriarAsync(SalvarUsuarioDto dto, CancellationToken cancellationToken)
    {
        if (await vinculos.ObterIdUsuarioAsync(identidade.IdIdentidade, cancellationToken) is not null)
            throw new ConflitoException("Usuário já cadastrado.");
        var usuario = new Usuario(dto.Nome);
        await repository.AdicionarAsync(usuario, cancellationToken);
        await vinculos.AssociarAsync(identidade.IdIdentidade, usuario.Id, cancellationToken);
        await unitOfWork.SalvarAsync(cancellationToken);
        return usuario.Adapt<UsuarioDto>(config);
    }
    public async Task<Usuario> ObterAtivoAsync(CancellationToken cancellationToken)
    {
        var usuario = await repository.ObterAsync(atual.IdUsuario, cancellationToken) ?? throw new NaoEncontradoException();
        usuario.GarantirAtivo();
        return usuario;
    }
    public async Task<UsuarioDto> ObterAsync(CancellationToken cancellationToken) => (await ObterAtivoAsync(cancellationToken)).Adapt<UsuarioDto>(config);
    public async Task<UsuarioDto> AtualizarAsync(SalvarUsuarioDto dto, CancellationToken cancellationToken)
    {
        var usuario = await ObterAtivoAsync(cancellationToken);
        usuario.Atualizar(dto.Nome);
        await unitOfWork.SalvarAsync(cancellationToken);
        return usuario.Adapt<UsuarioDto>(config);
    }
    public async Task<PreferenciasDoUsuarioDto> AtualizarPreferenciasAsync(PreferenciasDoUsuarioDto dto, CancellationToken cancellationToken)
    {
        var usuario = await ObterAtivoAsync(cancellationToken);
        usuario.AtualizarPreferencias(dto.MetaDiariaEmMinutos, dto.Idioma);
        await unitOfWork.SalvarAsync(cancellationToken);
        return usuario.Preferencias.Adapt<PreferenciasDoUsuarioDto>(config);
    }
    public async Task DesativarAsync(CancellationToken cancellationToken)
    {
        var usuario = await ObterAtivoAsync(cancellationToken);
        usuario.Desativar();
        await unitOfWork.SalvarAsync(cancellationToken);
    }
}
