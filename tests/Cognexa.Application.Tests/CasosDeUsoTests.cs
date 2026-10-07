using Cognexa.Application.Anotacoes;
using Cognexa.Application.Biblioteca;
using Cognexa.Application.Compartilhado;
using Cognexa.Application.Conhecimento;
using Cognexa.Application.Inteligencia;
using Cognexa.Application.Revisoes;
using Cognexa.Application.Usuarios;
using Cognexa.Domain.Anotacoes;
using Cognexa.Domain.Biblioteca;
using Cognexa.Domain.Compartilhado;
using Cognexa.Domain.Revisoes;
using Microsoft.Extensions.DependencyInjection;
namespace Cognexa.Application.Tests;

public sealed class CasosDeUsoTests : IDisposable
{
    private readonly ServiceProvider _services;
    private readonly UsuarioAtualTeste _usuario = new();
    private readonly UnitOfWorkTeste _uow = new();
    private readonly TempoTeste _tempo = new();
    private readonly InteligenciaTeste _ia = new();
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
        services.AddSingleton<IInteligenciaProvider>(_ia);
        services.AddSingleton<IBuscaSemanticaProvider>(_ia);
        services.AddSingleton<IAgendadorDeRevisao, AgendadorTeste>();
        services.AddSingleton<IRevisaoRepository, RevisaoRepositoryTeste>();
        _services = services.BuildServiceProvider();
    }
    private T Obter<T>() where T : notnull => _services.GetRequiredService<T>();
    private Task<LivroDto> Livro() => Obter<LivroAppService>().CriarAsync(new("Livro", ["Autor"], 100), default);
    private async Task<AnotacaoDto> Anotacao()
    {
        var livro = await Livro();
        return await Obter<AnotacaoAppService>().CriarAsync(new(livro.Id, null, TipoDeAnotacao.Insight, "Original", "Comentário", 10), default);
    }
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
    public async Task LivroAnotacaoEFontesDeOutroUsuarioNaoSaoAcessiveis()
    {
        var anotacao = await Anotacao();
        _usuario.IdUsuario = Guid.NewGuid();
        await Assert.ThrowsAsync<NaoEncontradoException>(() => Obter<LivroAppService>().ObterAsync(anotacao.IdLivro));
        await Assert.ThrowsAsync<NaoEncontradoException>(() => Obter<AnotacaoAppService>().ObterAsync(anotacao.Id));
        await Assert.ThrowsAsync<NaoEncontradoException>(() => Obter<AnotacaoAppService>().RemoverAsync(anotacao.Id));
        Assert.Empty(await Obter<AnotacaoAppService>().ListarAsync(new()));
        await Assert.ThrowsAsync<NaoEncontradoException>(() => Obter<AprendizadoAppService>().CriarAsync(new("Síntese", [anotacao.Id], []), default));
    }
    [Fact]
    public async Task AnotacaoValidaCapituloEPaginaDoLivro()
    {
        var livro = await Livro();
        var outro = await Livro();
        var capitulo = await Obter<LivroAppService>().CriarCapituloAsync(outro.Id, new("Capítulo", 1), default);
        await Assert.ThrowsAsync<NaoEncontradoException>(() => Obter<AnotacaoAppService>().CriarAsync(new(livro.Id, capitulo.Id, TipoDeAnotacao.Insight, null, "Comentário"), default));
        await Assert.ThrowsAsync<RegraDeDominioException>(() => Obter<AnotacaoAppService>().CriarAsync(new(livro.Id, null, TipoDeAnotacao.Insight, null, "Comentário", 101), default));
    }
    [Fact]
    public async Task FiltrosEncontramAnotacoesEConhecimentoPorFonteLivroEConceito()
    {
        var anotacao = await Anotacao();
        var outra = await Anotacao();
        Assert.Single(await Obter<AnotacaoAppService>().ListarAsync(new(IdLivro: anotacao.IdLivro, Tipo: TipoDeAnotacao.Insight, Pagina: 10)));
        Assert.Empty(await Obter<AnotacaoAppService>().ListarAsync(new(Tipo: TipoDeAnotacao.Duvida)));
        var conceito = await Obter<ConceitoAppService>().CriarAsync(new("Conceito"), default);
        var aprendizado = await Obter<AprendizadoAppService>().CriarAsync(new("Síntese", [anotacao.Id, outra.Id], [conceito.Id]), default);
        Assert.Equal(2, aprendizado.Fontes.Length);
        Assert.Single(await Obter<AprendizadoAppService>().ListarAsync(new(IdLivro: outra.IdLivro, IdConceito: conceito.Id, IdAnotacao: anotacao.Id)));
        var aplicacao = await Obter<ConexoesAppService>().CriarAplicacaoAsync(new(aprendizado.Id, "Aplicar"), default);
        Assert.Equal(aprendizado.Id, aplicacao.IdAprendizado);
    }
    [Fact]
    public async Task SugestaoNaoGravaNemSobrescreveOriginalERejeitaFontesInventadas()
    {
        var anotacao = await Anotacao();
        var gravacoes = _uow.Gravacoes;
        var pedido = new PedidoInteligenciaDto(OperacaoInteligente.SugerirAprendizado, [anotacao.Id], [], []);
        var resultado = await Obter<InteligenciaAppService>().SugerirAsync(pedido, default);
        Assert.True(resultado.GeradoPorIA);
        Assert.True(resultado.RequerConfirmacao);
        Assert.Equal(anotacao.Id, resultado.Fontes.Single().Id);
        Assert.Equal(gravacoes, _uow.Gravacoes);
        Assert.Equal("Original", (await Obter<AnotacaoAppService>().ObterAsync(anotacao.Id)).TrechoOriginal);
        _ia.FonteInventada = Guid.NewGuid();
        await Assert.ThrowsAsync<ServicoIndisponivelException>(() => Obter<InteligenciaAppService>().SugerirAsync(pedido, default));
    }
    [Fact]
    public async Task ResumoEBuscaSoRecebemFontesDoUsuarioAtual()
    {
        var anotacao = await Anotacao();
        await Anotacao();
        await Obter<InteligenciaAppService>().SugerirAsync(new(OperacaoInteligente.ResumirLivro, [], [], [], anotacao.IdLivro), default);
        Assert.Equal(anotacao.Id, _ia.FontesRecebidas.Single().Id);
        await Obter<InteligenciaAppService>().BuscarAsync("Consulta", 10, default);
        Assert.Equal(_usuario.IdUsuario, _ia.UsuarioDaBusca);
        _usuario.IdUsuario = Guid.NewGuid();
        await Assert.ThrowsAsync<NaoEncontradoException>(() => Obter<InteligenciaAppService>().SugerirAsync(new(OperacaoInteligente.SugerirAprendizado, [anotacao.Id], [], []), default));
    }
    [Fact]
    public async Task RevisaoUsaAgendadorSubstituivelEGravaHistorico()
    {
        var anotacao = await Anotacao();
        var aprendizado = await Obter<AprendizadoAppService>().CriarAsync(new("Síntese", [anotacao.Id], []), default);
        var criada = await Obter<RevisaoAppService>().CriarAsync(new(aprendizado.Id, "Pergunta?", "Resposta"), default);
        Assert.Single(await Obter<RevisaoAppService>().ListarDevidasAsync(null, null, 100, default));
        var revisao = await Obter<RevisaoAppService>().RegistrarAsync(criada.Revisao.Id, new(ResultadoDaRevisao.Bom, "Resposta"), default);
        Assert.Equal(_tempo.Agora.AddDays(3), revisao.ProximaRevisao);
        Assert.Empty(await Obter<RevisaoAppService>().ListarDevidasAsync(null, null, 100, default));
        var historicos = (RepositoryEmMemoria<HistoricoDeRevisao>)Obter<IRepository<HistoricoDeRevisao>>();
        Assert.Single(historicos.Entidades);
        _tempo.Agora = _tempo.Agora.AddDays(3);
        await Obter<RevisaoAppService>().RegistrarAsync(criada.Revisao.Id, new(ResultadoDaRevisao.Errou, null), default);
        Assert.Equal(2, historicos.Entidades.Count);
        _usuario.IdUsuario = Guid.NewGuid();
        await Assert.ThrowsAsync<NaoEncontradoException>(() => Obter<RevisaoAppService>().RegistrarAsync(criada.Revisao.Id, new(ResultadoDaRevisao.Bom, null), default));
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
