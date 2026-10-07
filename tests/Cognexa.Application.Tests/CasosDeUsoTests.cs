using Cognexa.Application.Biblioteca;
using Cognexa.Application.Compartilhado;
using Cognexa.Application.Usuarios;
using Cognexa.Domain.Biblioteca;
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
    private Task<LivroDto> Livro() => Obter<LivroAppService>().CriarAsync(new("Livro", ["Autor"], 100), default);
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
    [Fact]
    public async Task ContratoCrudMantemIdentidadeGeradaEAplicaFiltroNaAtualizacao()
    {
        ICrudBasicoAppService<LivroDto, LivroSearchDto, Livro> crud = Obter<LivroAppService>();
        var idFornecido = Guid.NewGuid();
        var criado = await crud.CriarAsync(new(idFornecido, "Livro", ["Autor"], 100, null, null, null));
        Assert.NotEqual(idFornecido, criado.Id);
        var alterado = await crud.AtualizarAsync(criado.Id, criado with
        {
            Titulo = "Novo"
        });
        Assert.Equal(criado.Id, alterado.Id);
        Assert.Equal("Novo", alterado.Titulo);
        _usuario.IdUsuario = Guid.NewGuid();
        await Assert.ThrowsAsync<NaoEncontradoException>(() => crud.AtualizarAsync(criado.Id, criado with { Titulo = "Inválido" }));
    }
    public void Dispose() => _services.Dispose();
}
