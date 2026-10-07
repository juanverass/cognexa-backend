using Cognexa.Application.Compartilhado;
using Cognexa.Application.Usuarios;
using Cognexa.Domain.Compartilhado;
using Microsoft.Extensions.DependencyInjection;
namespace Cognexa.Application.Tests;

public sealed class CasosDeUsoTests : IDisposable
{
    private readonly ServiceProvider _services;
    private readonly UsuarioAtualTeste _usuario = new();
    private readonly UnitOfWorkTeste _uow = new();
    private readonly TempoTeste _tempo = new();
    public CasosDeUsoTests()
    {
        var services = new ServiceCollection().AddApplication();
        services.AddSingleton(typeof(IRepository<>), typeof(RepositoryEmMemoria<>));
        var vinculo = new VinculoTeste(_usuario);
        services.AddSingleton<IIdentidadeAtual>(vinculo);
        services.AddSingleton<IVinculoDeIdentidadeRepository>(vinculo);
        services.AddSingleton<IUsuarioAtual>(_usuario);
        services.AddSingleton<IUnitOfWork>(_uow);
        services.AddSingleton<TimeProvider>(_tempo);
        _services = services.BuildServiceProvider();
    }
    private T Obter<T>() where T : notnull => _services.GetRequiredService<T>();
    [Fact]
    public async Task IdentidadeEPreferenciasPertencemAoUsuarioAtual()
    {
        var usuario = await Obter<UsuarioAppService>().CriarAsync(new("João"), default);
        Assert.Equal(_usuario.IdUsuario, usuario.Id);
        var preferencias = await Obter<UsuarioAppService>().AtualizarPreferenciasAsync(new(30, "en"), default);
        Assert.Equal(30, preferencias.MetaDiariaEmMinutos);
        await Assert.ThrowsAsync<ConflitoException>(() => Obter<UsuarioAppService>().CriarAsync(new("Outro"), default));
        _usuario.IdUsuario = Guid.NewGuid();
        await Assert.ThrowsAsync<NaoEncontradoException>(() => Obter<UsuarioAppService>().ObterAsync(default));
    }
    public void Dispose() => _services.Dispose();
}
