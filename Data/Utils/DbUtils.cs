namespace Data.Utils;

public static class DbUtils
{
    public static async Task EnsureOpenAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);
    }

    public static (string filtro, DynamicParameters parametros) MontarBase(Guid? estacaoId)
    {
        Guid StatusFinalizada = Guid.Parse(Constantes.OrdemServicoStatusFinalizada);
        Guid StatusCancelada = Guid.Parse(Constantes.OrdemServicoStatusCancelada);

        var filtro = estacaoId.HasValue ? "AND os.EstacaoId = @EstacaoId" : string.Empty;
        var parametros = new DynamicParameters();

        if (estacaoId.HasValue)
            parametros.Add("@EstacaoId", estacaoId.Value);

        parametros.Add("@AnoAtual", DateTime.Now.Year);
        parametros.Add("@MesAtual", DateTime.Now.Month);
        parametros.Add("@DataCorte", DateTime.Now.AddMonths(-6));
        parametros.Add("@Hoje", DateTime.Today);
        parametros.Add("@StatusFinalizada", StatusFinalizada);
        parametros.Add("@StatusCancelada", StatusCancelada);

        return (filtro, parametros);
    }
}
