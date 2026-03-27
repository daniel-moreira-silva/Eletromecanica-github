namespace Core.Models.DashboardAggregate;

public class DashboardOsAtrasadaDto
{
    public string Numero { get; set; } = string.Empty;
    public string NomeAtivo { get; set; } = string.Empty;
    public int DiasAtraso { get; set; }
    public string Motivacao { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}