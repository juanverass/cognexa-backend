using Cognexa.Application.Revisoes;
using Cognexa.Domain.Conhecimento;
using Cognexa.Domain.Revisoes;
using Cognexa.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;
namespace Cognexa.Infrastructure.Revisoes;

public sealed class RevisaoRepository(CognexaDbContext contexto) : Repository<Revisao>(contexto), IRevisaoRepository
{
    public async Task<IReadOnlyList<Revisao>> ListarDevidasAsync(Guid idUsuario, DateTimeOffset agora, Guid? idLivro, Guid? idConceito, int limite, CancellationToken ct)
    {
        var query = from revisao in Contexto.Set<Revisao>()
                    join pergunta in Contexto.Set<PerguntaDeRevisao>() on revisao.IdPergunta equals pergunta.Id
                    join aprendizado in Contexto.Set<Aprendizado>() on pergunta.IdAprendizado equals aprendizado.Id
                    where revisao.IdUsuario == idUsuario && revisao.ProximaRevisao <= agora
                       && (!idLivro.HasValue || aprendizado.Fontes.Any(f => f.IdLivro == idLivro))
                       && (!idConceito.HasValue || aprendizado.Conceitos.Any(c => c.IdConceito == idConceito))
                    select revisao;
        return await query.OrderBy(x => x.ProximaRevisao).ThenBy(x => x.Id).Take(limite).ToListAsync(ct);
    }
    public async Task<IReadOnlyList<HistoricoDeRevisao>> ListarHistoricoAsync(Guid idUsuario, Guid idRevisao, int pagina, CancellationToken ct) =>
        await Contexto.Set<HistoricoDeRevisao>().AsNoTracking().Where(x => x.IdUsuario == idUsuario && x.IdRevisao == idRevisao).OrderBy(x => x.Data).ThenBy(x => x.Id).Skip((pagina - 1) * 100).Take(100).ToListAsync(ct);
}
