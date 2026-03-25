namespace Business.Dtos;

public sealed class EquipamentoCompletoDto : Equipamento
{
    public EquipamentoCompletoDto()
    {
    }
    public EquipamentoCompletoDto(Equipamento equipamento)
    {
        Id = equipamento.Id;
        Nome = equipamento.Nome;
        EstacaoId = equipamento.EstacaoId;
        TipoEquipamentoId = equipamento.TipoEquipamentoId;
        EquipamentoPrincipalId = equipamento.EquipamentoPrincipalId;
        TipoEquipamento = equipamento.TipoEquipamento;
        Estacao = equipamento.Estacao;
        Tag = equipamento.Tag;
        Fabricante = equipamento.Fabricante;
        Modelo = equipamento.Modelo;
        NumeroSerie = equipamento.NumeroSerie;
        Observacoes = equipamento.Observacoes;
        DataCriacao = equipamento.DataCriacao;
        Ativo = equipamento.Ativo;
    }
    public Bomba? Bomba { get; set; }
    public Motor? Motor { get; set; }
    public CLP? CLP { get; set; }
    public Nobreak? Nobreak { get; set; }
    public MedidorVazao? MedidorVazao { get; set; }
    public List<CaracteristicaEquipamento> Caracteristicas { get; set; } = [];
    public List<EquipamentoComponenteDto>? Componentes { get; set; }
    public List<RegraPreventiva> RegrasPreventivas { get; set; } = [];
}