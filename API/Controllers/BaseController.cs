namespace API.Controllers;

public class BaseController(ILogger<BaseController> logger) : Controller
{
    protected void LogError(Exception ex, string mensagem)
    {
        var usuario = Request.Headers["Usuario"].ToString();
        var logger = LogManager.GetCurrentClassLogger();
        var eventInfo = new LogEventInfo(NLog.LogLevel.Error, logger.Name, mensagem)
        {
            Exception = ex
        };
        eventInfo.Properties.Add("usuario", usuario);
        logger.Log(eventInfo);
    }

    protected void LogExecucao(string mensagem)
    {
        try
        {
            var usuario = Request.Headers["Usuario"].ToString();
            var logger = LogManager.GetCurrentClassLogger();
            var eventInfo = new LogEventInfo(NLog.LogLevel.Info, logger.Name, mensagem);
            eventInfo.Properties.Add("usuario", usuario);
            logger.Log(eventInfo);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao registrar log de execução");
        }
    }
}
