namespace Worker.Services;

public interface IJobExecutor
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}
