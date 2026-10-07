using Cognexa.Application.Usuarios;
using Cognexa.Domain.Compartilhado;
using Cognexa.Domain.Usuarios;
using Cognexa.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Cognexa.Infrastructure.Identidade;

public sealed class VinculoDeIdentidade : EntidadeBase
{
    private VinculoDeIdentidade()
    {
    }
    internal VinculoDeIdentidade(Guid idIdentidade, Guid idUsuario)
    {
        IdIdentidade = idIdentidade;
        IdUsuario = idUsuario;
    }
    public Guid IdIdentidade
    {
        get; private set;
    }
    public Guid IdUsuario
    {
        get; private set;
    }
}
public sealed class VinculoDeIdentidadeConfiguration : IEntityTypeConfiguration<VinculoDeIdentidade>
{
    public void Configure(EntityTypeBuilder<VinculoDeIdentidade> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.HasIndex(x => x.IdIdentidade).IsUnique();
        b.HasIndex(x => x.IdUsuario).IsUnique();
        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.IdUsuario).OnDelete(DeleteBehavior.Restrict);
    }
}
public sealed class VinculoDeIdentidadeRepository(CognexaDbContext contexto) : IVinculoDeIdentidadeRepository
{
    public async Task<Guid?> ObterIdUsuarioAsync(Guid idIdentidade, CancellationToken cancellationToken) =>
        await contexto.Set<VinculoDeIdentidade>().AsNoTracking().Where(x => x.IdIdentidade == idIdentidade).Select(x => (Guid?)x.IdUsuario).SingleOrDefaultAsync(cancellationToken);
    public async Task AssociarAsync(Guid idIdentidade, Guid idUsuario, CancellationToken cancellationToken) =>
        await contexto.Set<VinculoDeIdentidade>().AddAsync(new(idIdentidade, idUsuario), cancellationToken);
}
