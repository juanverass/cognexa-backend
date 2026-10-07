using System.Linq.Expressions;
using Cognexa.Application.Compartilhado;
using Cognexa.Domain.Biblioteca;
using Cognexa.Domain.Compartilhado;
using Mapster;
namespace Cognexa.Application.Biblioteca;

public sealed class LivroAppService(IRepository<Livro> livros, IRepository<Leitura> leituras, IRepository<Capitulo> capitulos, IUnitOfWork unitOfWork, IUsuarioAtual atual, TypeAdapterConfig config, TimeProvider tempo)
    : CrudBasicoAppService<LivroDto, LivroSearchDto, Livro>(livros, unitOfWork)
{
    protected override Task<LivroDto> CriarDtoAsync(LivroDto dto, CancellationToken cancellationToken) => CriarAsync(new SalvarLivroDto(dto.Titulo, dto.Autores, dto.TotalDePaginas, dto.Isbn, dto.Edicao, dto.Capa), cancellationToken);
    protected override Task<LivroDto> AtualizarDtoAsync(Guid id, LivroDto dto, CancellationToken cancellationToken) => AtualizarAsync(id, new SalvarLivroDto(dto.Titulo, dto.Autores, dto.TotalDePaginas, dto.Isbn, dto.Edicao, dto.Capa), cancellationToken);

    protected override Expression<Func<Livro, bool>> Filtro(LivroSearchDto busca) => x => x.IdUsuario == atual.IdUsuario && (busca.Titulo == null || x.Titulo.Contains(busca.Titulo));
    protected override Expression<Func<Livro, bool>> FiltroPorId(Guid id) => x => x.Id == id && x.IdUsuario == atual.IdUsuario;
    protected override LivroDto Converter(Livro entidade) => entidade.Adapt<LivroDto>(config);
    public async Task<Livro> ExigirLivroAsync(Guid id, CancellationToken cancellationToken) => await ObterEntidadeAsync(id, cancellationToken);
    public async Task<LivroDto> CriarAsync(SalvarLivroDto dto, CancellationToken cancellationToken)
    {
        var livro = new Livro(atual.IdUsuario, dto.Titulo, dto.Autores, dto.TotalDePaginas, dto.Isbn, dto.Edicao, dto.Capa);
        await Repository.AdicionarAsync(livro, cancellationToken);
        await leituras.AdicionarAsync(new Leitura(atual.IdUsuario, livro.Id), cancellationToken);
        await UnitOfWork.SalvarAsync(cancellationToken);
        return Converter(livro);
    }
    public async Task<LivroDto> AtualizarAsync(Guid id, SalvarLivroDto dto, CancellationToken cancellationToken)
    {
        var livro = await ExigirLivroAsync(id, cancellationToken);
        var leitura = await ObterLeituraEntidadeAsync(id, cancellationToken);
        if (dto.TotalDePaginas.HasValue && (dto.TotalDePaginas < leitura.PaginaAtual || (leitura.Status == StatusDaLeitura.Concluido && dto.TotalDePaginas != leitura.PaginaAtual)))
            throw new ConflitoException("Total de páginas conflita com o progresso registrado.");
        livro.Atualizar(dto.Titulo, dto.Autores, dto.TotalDePaginas, dto.Isbn, dto.Edicao, dto.Capa);
        await UnitOfWork.SalvarAsync(cancellationToken);
        return Converter(livro);
    }
    private async Task<Leitura> ObterLeituraEntidadeAsync(Guid idLivro, CancellationToken cancellationToken) =>
        (await leituras.ListarAsync(x => x.IdLivro == idLivro && x.IdUsuario == atual.IdUsuario, 1, cancellationToken)).FirstOrDefault() ?? throw new NaoEncontradoException();
    public async Task<LeituraDto> ObterLeituraAsync(Guid idLivro, CancellationToken cancellationToken) => (await ObterLeituraEntidadeAsync(idLivro, cancellationToken)).Adapt<LeituraDto>(config);
    public async Task<IReadOnlyList<LeituraDto>> ListarLeiturasAsync(StatusDaLeitura? status, CancellationToken cancellationToken)
    {
        if (status.HasValue && !Enum.IsDefined(status.Value))
            throw new RegraDeDominioException("Estado inválido.");
        return (await leituras.ListarAsync(x => x.IdUsuario == atual.IdUsuario && (!status.HasValue || x.Status == status), 500, cancellationToken)).Select(x => x.Adapt<LeituraDto>(config)).ToArray();
    }
    public async Task<LeituraDto> AtualizarLeituraAsync(Guid idLivro, SalvarLeituraDto dto, CancellationToken cancellationToken)
    {
        var livro = await ExigirLivroAsync(idLivro, cancellationToken);
        var leitura = await ObterLeituraEntidadeAsync(idLivro, cancellationToken);
        leitura.Atualizar(dto.Status, dto.PaginaAtual, livro.TotalDePaginas, tempo.GetUtcNow());
        await UnitOfWork.SalvarAsync(cancellationToken);
        return leitura.Adapt<LeituraDto>(config);
    }
    public async Task<CapituloDto> CriarCapituloAsync(Guid idLivro, CriarCapituloDto dto, CancellationToken cancellationToken)
    {
        await ExigirLivroAsync(idLivro, cancellationToken);
        if ((await capitulos.ListarAsync(x => x.IdLivro == idLivro && x.Ordem == dto.Ordem, 1, cancellationToken)).Count > 0)
            throw new ConflitoException("Já existe capítulo nesta ordem.");
        var capitulo = new Capitulo(atual.IdUsuario, idLivro, dto.Titulo, dto.Ordem);
        await capitulos.AdicionarAsync(capitulo, cancellationToken);
        await UnitOfWork.SalvarAsync(cancellationToken);
        return capitulo.Adapt<CapituloDto>(config);
    }
    public async Task<IReadOnlyList<CapituloDto>> ListarCapitulosAsync(Guid idLivro, CancellationToken cancellationToken)
    {
        await ExigirLivroAsync(idLivro, cancellationToken);
        return (await capitulos.ListarAsync(x => x.IdLivro == idLivro && x.IdUsuario == atual.IdUsuario, 500, cancellationToken)).OrderBy(x => x.Ordem).Select(x => x.Adapt<CapituloDto>(config)).ToArray();
    }
    public async Task ExigirCapituloAsync(Guid idLivro, Guid idCapitulo, CancellationToken cancellationToken)
    {
        if ((await capitulos.ListarAsync(x => x.Id == idCapitulo && x.IdLivro == idLivro && x.IdUsuario == atual.IdUsuario, 1, cancellationToken)).Count == 0)
            throw new NaoEncontradoException();
    }
}
