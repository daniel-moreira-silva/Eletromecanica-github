namespace Core.Utils;

public static class RegraPreventivaUtils
{
    public static DateTime CalcularProximaExecucao(DateTime dataInicio, int intervalo, EUnidadePeriodoPreventivo unidade)
    {
        return unidade switch
        {
            EUnidadePeriodoPreventivo.Dia => dataInicio.AddDays(intervalo),
            EUnidadePeriodoPreventivo.Semana => dataInicio.AddDays(intervalo * 7),
            EUnidadePeriodoPreventivo.Mes => dataInicio.AddMonths(intervalo),
            EUnidadePeriodoPreventivo.Ano => dataInicio.AddYears(intervalo),
            _ => dataInicio
        };
    }
}
