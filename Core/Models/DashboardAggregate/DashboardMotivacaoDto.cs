namespace Core.Models.DashboardAggregate;

public class DashboardMotivacaoDto
{
    public int CorretivasMes { get; set; }
    public int PreventivasMes { get; set; }
    public int PreditivasMes { get; set; }
    public List<DashboardSerieMotivacaoDto> Serie { get; set; } = [];
}

public class DashboardSerieMotivacaoDto
{
    public string Mes { get; set; } = string.Empty;
    public int Corretivas { get; set; }
    public int Preventivas { get; set; }
    public int Preditivas { get; set; }
}