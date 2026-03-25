namespace Business.Services;

public class EquipamentoService(
        DbConnection connection,
        IEquipamentoRepository repository,
        ICaracteristicaEquipamentoRepository caracteristicaRepository,
        IBombaRepository bombaRepository,
        IMotorRepository motorRepository,
        ICLPRepository clpRepository,
        IMedidorVazaoRepository medidorVazaoRepository,
        INobreakRepository nobreakRepository,
        IRegraPreventivaRepository regraPreventivaEquipamentoRepository,
        ILogger<EquipamentoService> logger) : IEquipamentoService
{
    public async Task<Guid?> AddAsync(EquipamentoCompletoDto equipamento, CancellationToken cancellationToken)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var equipamentoId = await repository.AddAsync(equipamento, transaction, cancellationToken);

            if (equipamento.Bomba is not null)
            {
                equipamento.Bomba.EquipamentoId = equipamentoId;
                await bombaRepository.AddAsync(equipamento.Bomba, transaction, cancellationToken);
            }

            if (equipamento.Motor is not null)
            {
                equipamento.Motor.EquipamentoId = equipamentoId;
                await motorRepository.AddAsync(equipamento.Motor, transaction, cancellationToken);
            }

            if (equipamento.CLP is not null)
            {
                equipamento.CLP.EquipamentoId = equipamentoId;
                await clpRepository.AddAsync(equipamento.CLP, transaction, cancellationToken);
            }

            if (equipamento.Nobreak is not null)
            {
                equipamento.Nobreak.EquipamentoId = equipamentoId;
                await nobreakRepository.AddAsync(equipamento.Nobreak, transaction, cancellationToken);
            }

            if (equipamento.MedidorVazao is not null)
            {
                equipamento.MedidorVazao.EquipamentoId = equipamentoId;
                await medidorVazaoRepository.AddAsync(equipamento.MedidorVazao, transaction, cancellationToken);
            }

            if (equipamento.Caracteristicas.Count != 0)
            {
                foreach (var caracteristica in equipamento.Caracteristicas)
                    caracteristica.EquipamentoId = equipamentoId;

                await caracteristicaRepository.AddAsync(equipamento.Caracteristicas, transaction, cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);

            return equipamentoId;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(ex, "Erro ao adicionar equipamento completo.");
            return null;
        }
    }

    public async Task<List<Equipamento>> GetAllEquipmentsByEstacaoIdAsync(Guid estacaoId, bool? principal = null, CancellationToken cancellationToken = default)
        => await repository.GetAllPrincipalEquipmentsByEstacaoIdAsync(estacaoId, principal, null, cancellationToken);

    public async Task<Equipamento?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await repository.GetByIdAsync(id, null, cancellationToken);

    public async Task<EquipamentoCompletoDto?> GetCompleteEquipmentByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var equipment = await repository.GetByIdAsync(id, null, cancellationToken);

        if (equipment is null)
            return null;

        var components = equipment.EquipamentoPrincipalId.HasValue 
            ? null 
            : (await repository.GetAllComponentsByPrincipalEquipmentIdAsync(id, null, cancellationToken))
                .Select(x => (EquipamentoComponenteDto)x)
                .ToList();

        var regrasPreventivas = await regraPreventivaEquipamentoRepository.GetAllByEquipamentoIdAsync(id, null, cancellationToken);

        EquipamentoCompletoDto completeEquipment = new(equipment)
        {
            Caracteristicas = await caracteristicaRepository.GetByEquipamentoIdAsync(id, null, cancellationToken),
            Bomba = await bombaRepository.GetByEquipamentoIdAsync(id, null, cancellationToken),
            Motor = await motorRepository.GetByEquipamentoIdAsync(id, null, cancellationToken),
            CLP = await clpRepository.GetByEquipamentoIdAsync(id, null, cancellationToken),
            Nobreak = await nobreakRepository.GetByEquipamentoIdAsync(id, null, cancellationToken),
            MedidorVazao = await medidorVazaoRepository.GetByEquipamentoIdAsync(id, null, cancellationToken),
            Componentes = components,
            RegrasPreventivas = regrasPreventivas
        };

        return completeEquipment;
    }

    public async Task<ListaPaginada<Equipamento>> PaginatedGetAsync(EquipamentoFilter filter, CancellationToken cancellationToken)
        => await repository.PaginatedGetAsync(filter, null, cancellationToken);

    public async Task<List<TipoEquipamento>> GetAllTiposEquipamentoAsync(CancellationToken cancellationToken)
        => await repository.GetAllTiposEquipamentoAsync(null, cancellationToken);

    public async Task<bool> UpdateAsync(EquipamentoCompletoDto equipamento, CancellationToken cancellationToken)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await repository.UpdateAsync(equipamento, transaction, cancellationToken);

            if (equipamento.Bomba is not null)
                result = await bombaRepository.UpdateAsync(equipamento.Bomba, transaction, cancellationToken);

            if (equipamento.Motor is not null)
                result = await motorRepository.UpdateAsync(equipamento.Motor, transaction, cancellationToken);

            if (equipamento.CLP is not null)
                result = await clpRepository.UpdateAsync(equipamento.CLP, transaction, cancellationToken);

            if (equipamento.Nobreak is not null)
                result = await nobreakRepository.UpdateAsync(equipamento.Nobreak, transaction, cancellationToken);

            if (equipamento.MedidorVazao is not null)
                result = await medidorVazaoRepository.UpdateAsync(equipamento.MedidorVazao, transaction, cancellationToken);

            if (equipamento.Caracteristicas.Count != 0)
            {
                await caracteristicaRepository.RemoveAllByEquipmentIdAsync(equipamento.Id!.Value, transaction, cancellationToken);
                result = await caracteristicaRepository.AddAsync(equipamento.Caracteristicas, transaction, cancellationToken);
            }

            if (result)
                await transaction.CommitAsync(cancellationToken);
            else
                await transaction.RollbackAsync(cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(ex, "Erro ao atualizar equipamento completo.");
            return false;
        }
    }

    public async Task<bool> UpdateStatusAsync(Guid id, bool ativo, CancellationToken cancellationToken)
        => await repository.UpdateStatusAsync(id, ativo, null, cancellationToken);

    //public async Task<Guid?> AddAsync(RegraPreventivaEquipamento regra, CancellationToken cancellationToken)
    //{
    //    await DbUtils.EnsureOpenAsync(connection, cancellationToken);
    //    using var transaction = await connection.BeginTransactionAsync(cancellationToken);

    //    try
    //    {
    //        regra.ProximaExecucao = CalcularProximaExecucao(regra.DataInicio, regra.Intervalo, regra.UnidadePeriodo);

    //        var regraId = await regraPreventivaEquipamentoRepository.AddAsync(regra, transaction, cancellationToken);

    //        var servicosIds = (regra.ServicosSolicitados ?? [])
    //            .Select(x => x.ServicoSolicitadoId)
    //            .Distinct()
    //            .ToList();

    //        await regraPreventivaEquipamentoRepository.AddRangeRegraPreventivaEquipamentoServicosSolicitadoAsync(regraId, servicosIds, transaction, cancellationToken);

    //        await transaction.CommitAsync(cancellationToken);
    //        return regraId;
    //    }
    //    catch (Exception ex)
    //    {
    //        await transaction.RollbackAsync(cancellationToken);
    //        logger.LogError(ex, "Erro ao adicionar regra preventiva.");
    //        return null;
    //    }
    //}

    //public async Task<bool> UpdateAsync(RegraPreventivaEquipamento regra, CancellationToken cancellationToken)
    //{
    //    await DbUtils.EnsureOpenAsync(connection, cancellationToken);
    //    using var transaction = await connection.BeginTransactionAsync(cancellationToken);

    //    try
    //    {
    //        regra.ProximaExecucao = CalcularProximaExecucao(regra.DataInicio, regra.Intervalo, regra.UnidadePeriodo);

    //        var updated = await regraPreventivaEquipamentoRepository.UpdateAsync(regra, transaction, cancellationToken);
    //        if (!updated)
    //        {
    //            await transaction.RollbackAsync(cancellationToken);
    //            return false;
    //        }

    //        await regraPreventivaEquipamentoRepository.RemoveAllRegraPreventivaEquipamentoServicosSolicitadoByRegraIdAsync(regra.Id!.Value, transaction, cancellationToken);

    //        var servicosIds = (regra.ServicosSolicitados ?? [])
    //            .Select(x => x.ServicoSolicitadoId)
    //            .Distinct()
    //            .ToList();

    //        await regraPreventivaEquipamentoRepository.AddRangeRegraPreventivaEquipamentoServicosSolicitadoAsync(regra.Id.Value, servicosIds, transaction, cancellationToken);

    //        await transaction.CommitAsync(cancellationToken);
    //        return true;
    //    }
    //    catch (Exception ex)
    //    {
    //        await transaction.RollbackAsync(cancellationToken);
    //        logger.LogError(ex, "Erro ao atualizar regra preventiva.");
    //        return false;
    //    }
    //}

    //public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    //{
    //    try
    //    {
    //        return await regraPreventivaEquipamentoRepository.DeleteAsync(id, null, cancellationToken);
    //    }
    //    catch (Exception ex)
    //    {
    //        logger.LogError(ex, "Erro ao deletar regra preventiva.");
    //        return false;
    //    }
    //}

    //public async Task<List<RegraPreventivaEquipamento>> GetAllPreventivasByEquipamentoIdAsync(Guid equipamentoId, CancellationToken cancellationToken)
    //{
    //    var regras = await regraPreventivaEquipamentoRepository.GetAllByEquipamentoIdAsync(equipamentoId, null, cancellationToken);

    //    foreach (var regra in regras)
    //    {
    //        regra.ServicosSolicitados = await regraPreventivaEquipamentoRepository.GetAllRegraPreventivaEquipamentoServicosSolicitadoByRegraIdAsync(regra.Id!.Value, null, cancellationToken);
    //    }

    //    return regras;
    //}

    //private static DateTime CalcularProximaExecucao(DateTime dataInicio, int intervalo, EUnidadePeriodoPreventivo unidade)
    //{
    //    return unidade switch
    //    {
    //        EUnidadePeriodoPreventivo.Dia => dataInicio.AddDays(intervalo),
    //        EUnidadePeriodoPreventivo.Semana => dataInicio.AddDays(intervalo * 7),
    //        EUnidadePeriodoPreventivo.Mes => dataInicio.AddMonths(intervalo),
    //        EUnidadePeriodoPreventivo.Ano => dataInicio.AddYears(intervalo),
    //        _ => dataInicio
    //    };
    //}
}

