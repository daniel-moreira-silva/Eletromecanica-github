namespace Core.Models.Enums;

public enum EOrigemOS
{
    Manual,
    Agendamento,
    Alarme, // Pode ser utilizado para ordens de serviço geradas a partir de alarmes ou eventos do sistema
    Importacao // Pode ser utilizado para ordens de serviço criadas a partir de importações de dados ou integração com outros sistemas
}
