namespace Worker.Workers;

public sealed class SchedulerWorker(
    ILogger<SchedulerWorker> logger,
    IServiceScopeFactory scopeFactory,
    IOptions<SchedulerOptions> options) : BackgroundService
{
    private readonly SchedulerOptions _options = options.Value;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("SchedulerWorker iniciado em {Date}", DateTime.UtcNow);

        var cron = CronExpression.Parse(_options.Cron, CronFormat.Standard);
        var timeZone = ResolveTimeZone(_options.TimeZone);

        if (_options.RunOnStartup)
        {
            await RunJobSafelyAsync(stoppingToken);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var nowUtc = DateTime.UtcNow;
            var nextUtc = cron.GetNextOccurrence(nowUtc, timeZone);

            if (nextUtc is null)
            {
                logger.LogWarning("Nenhuma próxima execução encontrada para o cron {Cron}", _options.Cron);
                break;
            }

            var delay = nextUtc.Value - nowUtc;

            logger.LogInformation(
                "Próxima execução em {NextRunLocal} (cron: {Cron})",
                TimeZoneInfo.ConvertTime(nextUtc.Value, timeZone),
                _options.Cron);

            try
            {
                if (delay > TimeSpan.Zero)
                    await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("SchedulerWorker cancelado durante a espera.");
                break;
            }

            await RunJobSafelyAsync(stoppingToken);
        }
    }

    private async Task RunJobSafelyAsync(CancellationToken cancellationToken)
    {
        if (!await _semaphore.WaitAsync(0, cancellationToken))
        {
            logger.LogWarning("Job ignorado porque já existe uma execução em andamento.");
            return;
        }

        try
        {
            using var scope = scopeFactory.CreateScope();
            var executor = scope.ServiceProvider.GetRequiredService<IJobExecutor>();

            await executor.ExecuteAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Execução cancelada.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao executar job.");
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private static TimeZoneInfo ResolveTimeZone(string timeZoneId)
    {
        return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
    }
}