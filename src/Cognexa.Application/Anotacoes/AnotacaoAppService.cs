using System.Linq.Expressions;
using Cognexa.Application.Biblioteca;
using Cognexa.Application.Compartilhado;
using Cognexa.Domain.Anotacoes;
using Cognexa.Domain.Compartilhado;
using Mapster;
namespace Cognexa.Application.Anotacoes;

public record SalvarAnotacaoDto(Guid IdLivro, Guid? IdCapitulo, TipoDeAnotacao Tipo, string? TrechoOriginal, string? Comentario, int? Pagina = null, string? Localizacao = null, MetodoDeCaptura MetodoDeCaptura = MetodoDeCaptura.Manual, OrigemDoConteudo OrigemDoComentario = OrigemDoConteudo.Manual, string? Obra = null, string? Autor = null);
public record AnotacaoDto(Guid Id, Guid IdLivro, Guid? IdCapitulo, TipoDeAnotacao Tipo, string? TrechoOriginal, string? Comentario, int? Pagina, string? Localizacao, DateTimeOffset DataDeCriacao, MetodoDeCaptura MetodoDeCaptura = MetodoDeCaptura.Manual, OrigemDoConteudo OrigemDoComentario = OrigemDoConteudo.Manual, string? Obra = null, string? Autor = null, EstadoDePublicacao Publicacao = EstadoDePublicacao.Privado, EstadoDeModeracao Moderacao = EstadoDeModeracao.SemDenuncia);
public record AnotacaoSearchDto(Guid? IdLivro = null, TipoDeAnotacao? Tipo = null, int? Pagina = null, string? Localizacao = null, Guid? IdCapitulo = null, int Limite = 100) : SearchDto(Limite);
public sealed class MapeamentoAnotacao : IRegister
{
    public void Register(TypeAdapterConfig config) => config.NewConfig<Anotacao, AnotacaoDto>();
}
public sealed class AnotacaoAppService(IRepository<Anotacao> anotacoes, IUnitOfWork unitOfWork, IUsuarioAtual atual, LivroAppService biblioteca, TypeAdapterConfig config, TimeProvider tempo, IControleDeConteudo controle)
    : CrudBasicoAppService<AnotacaoDto, AnotacaoSearchDto, Anotacao>(anotacoes, unitOfWork)
{
    protected override Task<AnotacaoDto> CriarDtoAsync(AnotacaoDto dto, CancellationToken cancellationToken) => CriarAsync(new SalvarAnotacaoDto(dto.IdLivro, dto.IdCapitulo, dto.Tipo, dto.TrechoOriginal, dto.Comentario, dto.Pagina, dto.Localizacao, dto.MetodoDeCaptura, dto.OrigemDoComentario, dto.Obra, dto.Autor), cancellationToken);
    protected override Task<AnotacaoDto> AtualizarDtoAsync(Guid id, AnotacaoDto dto, CancellationToken cancellationToken) => AtualizarAsync(id, new SalvarAnotacaoDto(dto.IdLivro, dto.IdCapitulo, dto.Tipo, dto.TrechoOriginal, dto.Comentario, dto.Pagina, dto.Localizacao, dto.MetodoDeCaptura, dto.OrigemDoComentario, dto.Obra, dto.Autor), cancellationToken);

    protected override Expression<Func<Anotacao, bool>> Filtro(AnotacaoSearchDto busca) => x => x.IdUsuario == atual.IdUsuario
        && (!busca.IdLivro.HasValue || x.IdLivro == busca.IdLivro) && (!busca.Tipo.HasValue || x.Tipo == busca.Tipo)
        && (!busca.Pagina.HasValue || x.Pagina == busca.Pagina) && (busca.Localizacao == null || x.Localizacao == busca.Localizacao)
        && (!busca.IdCapitulo.HasValue || x.IdCapitulo == busca.IdCapitulo);
    protected override Expression<Func<Anotacao, bool>> FiltroPorId(Guid id) => x => x.Id == id && x.IdUsuario == atual.IdUsuario;
    protected override AnotacaoDto Converter(Anotacao entidade) => entidade.Adapt<AnotacaoDto>(config);
    public Task<Anotacao> ExigirAnotacaoAsync(Guid id, CancellationToken cancellationToken) => ObterEntidadeAsync(id, cancellationToken);
    private async Task ValidarLocalAsync(SalvarAnotacaoDto dto, CancellationToken cancellationToken)
    {
        var livro = await biblioteca.ExigirLivroAsync(dto.IdLivro, cancellationToken);
        if (dto.Pagina.HasValue && livro.TotalDePaginas.HasValue && dto.Pagina > livro.TotalDePaginas)
            throw new RegraDeDominioException("Página excede o total do livro.");
        if (dto.IdCapitulo.HasValue)
            await biblioteca.ExigirCapituloAsync(dto.IdLivro, dto.IdCapitulo.Value, cancellationToken);
    }
    public async Task<AnotacaoDto> CriarAsync(SalvarAnotacaoDto dto, CancellationToken cancellationToken)
    {
        await ValidarLocalAsync(dto, cancellationToken);
        var anotacao = new Anotacao(atual.IdUsuario, dto.IdLivro, dto.IdCapitulo, dto.Tipo, dto.TrechoOriginal, dto.Comentario, dto.Pagina, dto.Localizacao, tempo.GetUtcNow());
        anotacao.DefinirProveniencia(dto.MetodoDeCaptura, dto.OrigemDoComentario, dto.Obra, dto.Autor);
        return await controle.ExecutarAsync(atual.IdUsuario, dto.IdLivro, dto.TrechoOriginal, dto.Pagina, async () =>
        {
        await Repository.AdicionarAsync(anotacao, cancellationToken);
        await UnitOfWork.SalvarAsync(cancellationToken);
        return Converter(anotacao);
        }, cancellationToken);
    }
    public async Task<AnotacaoDto> AtualizarAsync(Guid id, SalvarAnotacaoDto dto, CancellationToken cancellationToken)
    {
        var anotacao = await ExigirAnotacaoAsync(id, cancellationToken);
        if (dto.IdLivro != anotacao.IdLivro)
            throw new ConflitoException("O livro da fonte não pode ser alterado.");
        await ValidarLocalAsync(dto, cancellationToken);
        return await controle.ExecutarAsync(atual.IdUsuario, dto.IdLivro, dto.TrechoOriginal == anotacao.TrechoOriginal && dto.Pagina == anotacao.Pagina && dto.Localizacao == anotacao.Localizacao ? null : dto.TrechoOriginal, dto.Pagina, async () =>
        {
        anotacao.Atualizar(dto.IdCapitulo, dto.Tipo, dto.TrechoOriginal, dto.Comentario, dto.Pagina, dto.Localizacao);
        anotacao.DefinirProveniencia(dto.MetodoDeCaptura, dto.OrigemDoComentario, dto.Obra, dto.Autor);
        await UnitOfWork.SalvarAsync(cancellationToken);
        return Converter(anotacao);
        }, cancellationToken);
    }
}
