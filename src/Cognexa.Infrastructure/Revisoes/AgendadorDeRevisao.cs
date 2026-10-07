using Cognexa.Application.Revisoes;
using Cognexa.Domain.Compartilhado;
using Cognexa.Domain.Revisoes;
namespace Cognexa.Infrastructure.Revisoes;

public sealed class AgendadorDeRevisao : IAgendadorDeRevisao
{
    public DateTimeOffset CalcularProxima(Revisao revisao, ResultadoDaRevisao resultado, DateTimeOffset agora)
    {
        if (!Enum.IsDefined(resultado))
            throw new RegraDeDominioException("Resultado inválido.");
        var baseDias = Math.Min(365, Math.Pow(2, Math.Min(revisao.AcertosConsecutivos, 9)));
        var dias = resultado switch
        {
            ResultadoDaRevisao.Errou => 1,
            ResultadoDaRevisao.Dificil => Math.Max(1, baseDias / 2),
            ResultadoDaRevisao.Facil => Math.Min(365, baseDias * 3),
            _ => baseDias * 2
        };
        return agora.AddDays(Math.Min(365, dias));
    }
}
