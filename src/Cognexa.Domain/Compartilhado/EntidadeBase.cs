namespace Cognexa.Domain.Compartilhado;

public abstract class EntidadeBase
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
}
public class RegraDeDominioException(string mensagem) : Exception(mensagem);
public class ConflitoException(string mensagem) : Exception(mensagem);
public static class Validacao
{
    public static string Texto(string? valor, string campo, int limite = 2000)
    {
        if (string.IsNullOrWhiteSpace(valor) || valor.Length > limite)
            throw new RegraDeDominioException($"{campo} deve conter entre 1 e {limite} caracteres.");
        return valor.Trim();
    }
    public static Guid Id(Guid valor, string campo)
    {
        if (valor == Guid.Empty)
            throw new RegraDeDominioException($"{campo} é obrigatório.");
        return valor;
    }
}

public enum OrigemDoConteudo
{
    Manual, InteligenciaArtificial
}
