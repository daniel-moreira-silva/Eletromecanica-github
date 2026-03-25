namespace Data.Utils;

public static class DocumentoUtils
{
    public static string NormalizeRel(string rel) => rel.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
    public static string BuildCaminhoRelativo(EEntidadeTipo tipo, Guid entidadeId)
    {
        var folder = tipo switch
        {
            EEntidadeTipo.Equipamento => EEntidadeTipo.Equipamento.GetDescription(),
            EEntidadeTipo.Estacao => EEntidadeTipo.Estacao.GetDescription(),
            _ => "outros"
        };

        return Path.Combine(folder, entidadeId.ToString("N"), "docs");
    }
}
