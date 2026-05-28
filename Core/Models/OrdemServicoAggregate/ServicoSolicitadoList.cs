using Core.Utils;

namespace Core.Models.OrdemServicoAggregate;

public class ServicoSolicitadoList : ServicoSolicitado
{
    public string PrioridadeDescricao => Prioridade.GetDescription();
}
