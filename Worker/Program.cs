var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<SchedulerOptions>(
    builder.Configuration.GetSection("Scheduler"));

builder.Logging.AddNLog();

builder.Services.AddBusinessServices(builder.Configuration);

builder.Services.AddScoped<IJobExecutor, JobExecutor>();
builder.Services.AddScoped<IProcessamentoRegraPreventivaService, ProcessamentoRegraPreventivaService>();

builder.Services.AddHostedService<SchedulerWorker>();

var host = builder.Build();
host.Run();