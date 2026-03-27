namespace Core.Models.DashboardAggregate;

public class DashboardDisponibilidadeDto
{
    public double Geral { get; set; }
    public List<DashboardDisponibilidadeAtivoDto> Ativos { get; set; } = [];
}

public class DashboardDisponibilidadeAtivoDto
{
    public string NomeAtivo { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;
    public double Disponibilidade { get; set; }
}
