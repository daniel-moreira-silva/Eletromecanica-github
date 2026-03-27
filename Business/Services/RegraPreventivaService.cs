namespace Business.Services;

public class RegraPreventivaService(IRegraPreventivaRepository repository, ILogger<RegraPreventivaService> logger, DbConnection connection) : IRegraPreventivaService
{
    public async Task<Guid?> AddAsync(RegraPreventiva regra, CancellationToken cancellationToken)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            regra.ProximoProcessamento = RegraPreventivaUtils.CalcularProximaExecucao(regra.DataInicio, regra.Intervalo, regra.UnidadePeriodo);

            var regraId = await repository.AddAsync(regra, transaction, cancellationToken);

            var servicosIds = (regra.ServicosSolicitados ?? [])
                .Select(x => x.ServicoSolicitadoId)
                .Distinct()
                .ToList();

            await repository.AddRangeRegraPreventivaServicosSolicitadoAsync(regraId, servicosIds, transaction, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return regraId;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(ex, "Erro ao adicionar regra preventiva.");
            return null;
        }
    }

    public async Task<bool> UpdateAsync(RegraPreventiva regra, CancellationToken cancellationToken)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await UpdateAsync(regra, transaction, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(ex, "Erro ao atualizar regra preventiva.");
            return false;
        }
    }

    public async Task<bool> UpdateAsync(RegraPreventiva regra, DbTransaction transaction, CancellationToken cancellationToken)
    {
        regra.ProximoProcessamento = RegraPreventivaUtils.CalcularProximaExecucao(regra.UltimoProcessamento ?? regra.DataInicio, regra.Intervalo, regra.UnidadePeriodo);

        var updated = await repository.UpdateAsync(regra, transaction, cancellationToken);

        if (!updated)
            return false;

        await repository.RemoveAllRegraPreventivaServicosSolicitadoByRegraIdAsync(regra.Id!.Value, transaction, cancellationToken);

        var servicosIds = (regra.ServicosSolicitados ?? []).Select(x => x.ServicoSolicitadoId).Distinct().ToList();

        await repository.AddRangeRegraPreventivaServicosSolicitadoAsync(regra.Id.Value, servicosIds, transaction, cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return await repository.DeleteAsync(id, null, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao deletar regra preventiva.");
            return false;
        }
    }

    public async Task<List<RegraPreventiva>> GetAllPreventivasByEquipamentoIdAsync(Guid equipamentoId, CancellationToken cancellationToken)
    {
        var regras = await repository.GetAllByEquipamentoIdAsync(equipamentoId, null, cancellationToken);

        foreach (var regra in regras)
        {
            regra.ServicosSolicitados = await repository.GetAllRegraPreventivaServicosSolicitadoByRegraIdAsync(regra.Id!.Value, null, cancellationToken);
        }

        return regras;
    }

    public async Task<bool> UpdateStatusEmProcessamentoAsync(Guid regraId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
        => await repository.UpdateStatusEmProcessamentoAsync(regraId, transaction, cancellationToken);

    public async Task<bool> UpdateStatusAguardandoProcessamentoAsync(Guid regraId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
        => await repository.UpdateStatusAguardandoProcessamentoAsync(regraId, transaction, cancellationToken);
}
