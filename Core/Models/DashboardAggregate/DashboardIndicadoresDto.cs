namespace Core.Models.DashboardAggregate;

public class DashboardIndicadoresDto
{
    public double Atual { get; set; }
    public List<DashboardSerieMensalDto> Serie { get; set; } = [];
}

public class DashboardSerieMensalDto
{
    public string Mes { get; set; } = string.Empty;
    public double Valor { get; set; }
}
