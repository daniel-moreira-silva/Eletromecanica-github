namespace Worker.Services;

public class JobExecutor(IRegraPreventivaRepository repository,
    IProcessamentoRegraPreventivaService processamentoRegraPreventivaService,
    ILogger<JobExecutor> logger) : IJobExecutor
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Iniciando processamento das regras preventivas em: {Date}", DateTime.UtcNow);

        var regras = await repository.GetAllByTodayAndAtivoAndAguardandoProcessamento(cancellationToken: cancellationToken);

        foreach (var regra in regras)
        {
            try
            {
                await processamentoRegraPreventivaService.ProcessarRegraAsync(regra, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Erro ao processar a regra preventiva {RegraId} para o equipamento {EquipamentoId}",
                    regra.Id,
                    regra.EquipamentoId);
            }
        }

        logger.LogInformation("Processamento das regras preventivas finalizado em: {Date}", DateTime.UtcNow);
    }
}