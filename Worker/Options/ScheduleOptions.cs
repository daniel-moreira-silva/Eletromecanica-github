namespace Worker.Options;

public sealed class SchedulerOptions
{
    public string Cron { get; set; } = default!;
    public string TimeZone { get; set; } = default!;
    public bool RunOnStartup { get; set; }
}