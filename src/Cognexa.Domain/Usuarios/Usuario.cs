using Cognexa.Domain.Compartilhado;
namespace Cognexa.Domain.Usuarios;

public sealed class Usuario : EntidadeBase
{
    private Usuario()
    {
    }
    public Usuario(string nome)
    {
        Nome = Validacao.Texto(nome, "Nome", 200);
    }
    public string Nome { get; private set; } = "";
    public bool Ativo { get; private set; } = true;
    public PreferenciasDoUsuario Preferencias { get; private set; } = new();
    public void Atualizar(string nome)
    {
        GarantirAtivo();
        Nome = Validacao.Texto(nome, "Nome", 200);
    }
    public void AtualizarPreferencias(int metaDiaria, string idioma)
    {
        GarantirAtivo();
        Preferencias = new(metaDiaria, idioma);
    }
    public void Desativar() => Ativo = false;
    public void GarantirAtivo()
    {
        if (!Ativo)
            throw new ConflitoException("Conta desativada.");
    }
}
public sealed class PreferenciasDoUsuario
{
    public PreferenciasDoUsuario() : this(20, "pt-BR") { }
    public PreferenciasDoUsuario(int metaDiaria, string idioma)
    {
        if (metaDiaria is < 1 or > 1440)
            throw new RegraDeDominioException("Meta diária deve estar entre 1 e 1440 minutos.");
        MetaDiariaEmMinutos = metaDiaria;
        Idioma = Validacao.Texto(idioma, "Idioma", 20);
    }
    public int MetaDiariaEmMinutos
    {
        get; private set;
    }
    public string Idioma { get; private set; } = "pt-BR";
}
