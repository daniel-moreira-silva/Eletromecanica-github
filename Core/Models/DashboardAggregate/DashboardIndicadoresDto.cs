namespace Core.Models.DashboardAggregate;

public class DashboardIndicadoresDto
{
    public double MttrAtual { get; set; }
    public double MtbfAtual { get; set; }
    public List<DashboardSerieMensalDto> SerieMttr { get; set; } = [];
    public List<DashboardSerieMensalDto> SerieMtbf { get; set; } = [];
}

public class DashboardSerieMensalDto
{
    public string Mes { get; set; } = string.Empty;
    public double Valor { get; set; }
}
