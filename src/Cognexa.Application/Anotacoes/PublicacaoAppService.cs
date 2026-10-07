using Cognexa.Application.Compartilhado;
using Cognexa.Domain.Anotacoes;
using Cognexa.Domain.Compartilhado;
namespace Cognexa.Application.Anotacoes;
public record CitacaoPublicaDto(Guid Id, string? TrechoOriginal, string? Comentario, string? Obra, string? Autor, int? Pagina, string? Localizacao, OrigemDoConteudo OrigemDoComentario);
public sealed class PublicacaoAppService(AnotacaoAppService privado, IRepository<Anotacao> anotacoes, IRepository<Denuncia> denuncias, IRepository<RegistroDePublicacao> auditoria, IUnitOfWork uow, IUsuarioAtual atual, ConteudoOptions options, IControleDeConteudo controle, TimeProvider tempo)
{
    private CitacaoPublicaDto Converter(Anotacao a) => new(a.Id, a.TrechoOriginal, a.Comentario, a.Obra, a.Autor, a.Pagina, a.Localizacao, a.OrigemDoComentario);
    public async Task<CitacaoPublicaDto> ObterAsync(Guid id, CancellationToken ct)
    {
        if (!options.GateAberto) throw new NaoEncontradoException();
        var a = (await anotacoes.ListarAsync(x => x.Id == id && x.Publicacao == EstadoDePublicacao.Publicado && x.Moderacao == EstadoDeModeracao.SemDenuncia, 1, ct)).FirstOrDefault() ?? throw new NaoEncontradoException();
        return Converter(a);
    }
    public async Task PrepararAsync(Guid id, bool publicar, CancellationToken ct)
    {
        var a = await privado.ExigirAnotacaoAsync(id, ct);
        await controle.ExecutarAsync(atual.IdUsuario, a.IdLivro, null, null, async () =>
        {
            await controle.ValidarPublicacaoAsync(atual.IdUsuario, a.IdLivro, a.Id, a.TrechoOriginal, ct);
            if (publicar) a.Publicar(options.GateAberto); else a.PrepararPublicacao();
            await auditoria.AdicionarAsync(new(a, atual.IdUsuario, tempo.GetUtcNow()), ct);
            await uow.SalvarAsync(ct);
            return true;
        }, ct);
    }
    public async Task DenunciarAsync(Guid id, MotivoDeDenuncia motivo, CancellationToken ct)
    {
        await ObterAsync(id, ct);
        var a = await anotacoes.ObterAsync(id, ct) ?? throw new NaoEncontradoException();
        a.Denunciar();
        await denuncias.AdicionarAsync(new(id, atual.IdUsuario, motivo, tempo.GetUtcNow()), ct);
        await auditoria.AdicionarAsync(new(a, atual.IdUsuario, tempo.GetUtcNow()), ct);
        await uow.SalvarAsync(ct);
    }
    public async Task RetirarAsync(Guid id, CancellationToken ct)
    {
        var a = await privado.ExigirAnotacaoAsync(id, ct);
        a.Retirar();
        await auditoria.AdicionarAsync(new(a, atual.IdUsuario, tempo.GetUtcNow()), ct);
        await uow.SalvarAsync(ct);
    }
}
