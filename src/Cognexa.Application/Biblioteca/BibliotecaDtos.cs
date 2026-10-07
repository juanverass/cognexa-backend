using Cognexa.Application.Compartilhado;
using Cognexa.Domain.Biblioteca;
using Mapster;
namespace Cognexa.Application.Biblioteca;

public record SalvarLivroDto(string Titulo, string[] Autores, int? TotalDePaginas = null, string? Isbn = null, string? Edicao = null, string? Capa = null);
public record LivroDto(Guid Id, string Titulo, string[] Autores, int? TotalDePaginas, string? Isbn, string? Edicao, string? Capa);
public record LivroSearchDto(string? Titulo = null, int Limite = 100) : SearchDto(Limite);
public record LeituraDto(Guid Id, Guid IdLivro, StatusDaLeitura Status, int PaginaAtual, string? Motivo, DateTimeOffset? DataDeInicio, DateTimeOffset? DataDeConclusao);
public record SalvarLeituraDto(StatusDaLeitura Status, int PaginaAtual);
public record CapituloDto(Guid Id, Guid IdLivro, string Titulo, int Ordem);
public record CriarCapituloDto(string Titulo, int Ordem);
public sealed class MapeamentoBiblioteca : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Livro, LivroDto>().Map(d => d.Autores, s => s.Autores.Select(a => a.Nome).ToArray());
        config.NewConfig<Leitura, LeituraDto>();
        config.NewConfig<Capitulo, CapituloDto>();
    }
}
