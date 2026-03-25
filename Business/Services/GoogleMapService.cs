namespace Business.Services;

public class GoogleMapService(IConfiguration configuration, IGoogleMapRepository repository) : IGoogleMapService
{
    public async Task<EnderecoDto> GetAsync(string? latLong = null, string? textoLivre = null, bool retentativa = false, CancellationToken cancellationToken = default)
    {
        var key = configuration["GoogleMaps:Key"] ?? string.Empty;
        var resultado = await repository.GetAsync(key, latLong, textoLivre, cancellationToken);
        if (resultado.Content != null)
        {
            //var endereco = ToEnderecoDto(resultado.Content);
            //if (!string.IsNullOrEmpty(endereco.Lat) && !string.IsNullOrEmpty(endereco.Long) && string.IsNullOrEmpty(endereco.Rua) && !retentativa)
            //    return await GetAsync(string.Concat(endereco.Lat, ",", endereco.Long), null, cancellationToken: cancellationToken);

            return ToEnderecoDto(resultado.Content);
        }

        throw new Exception("Erro ao pesquisar endereço através de coordenadas geográficas");
    }

    private static EnderecoDto ToEnderecoDto(GoogleMap google)
    {
        if (google is null)
            throw new Exception("Erro ao pesquisar endereço através de coordenadas geográficas");

        var result = (google.Results?.FirstOrDefault()) ?? throw new Exception("Erro ao pesquisar endereço através de coordenadas geográficas");
        var comps = result.AddressComponents ?? [];

        // Helpers
        static string FirstLong(IEnumerable<AddressComponent> components, params string[] types) =>
            components.FirstOrDefault(c => c.Types != null && types.Any(t => c.Types.Contains(t)))?.LongName ?? string.Empty;

        static string FirstShort(IEnumerable<AddressComponent> components, params string[] types) =>
            components.FirstOrDefault(c => c.Types != null && types.Any(t => c.Types.Contains(t)))?.ShortName ?? string.Empty;

        // Rua e número
        var rua = FirstLong(comps, "route");
        var numero = FirstLong(comps, "street_number");

        // Bairro: no BR costuma vir em sublocality_level_1; às vezes "neighborhood"
        var bairro =
            FirstLong(comps, "sublocality_level_1") ??
            FirstLong(comps, "sublocality") ??
            FirstLong(comps, "neighborhood") ??
            string.Empty;

        // Cidade: geralmente administrative_area_level_2 (município)
        // Fallback: "locality" (muitos países) ou "postal_town" (alguns casos)
        var cidade =
            FirstLong(comps, "administrative_area_level_2") ??
            FirstLong(comps, "locality") ??
            FirstLong(comps, "postal_town") ??
            string.Empty;

        // Estado: administrative_area_level_1. Para BR, short_name dá "SP", "RJ" etc.
        var estado = FirstShort(comps, "administrative_area_level_1");
        if (string.IsNullOrWhiteSpace(estado))
            estado = FirstLong(comps, "administrative_area_level_1");

        // CEP
        var cep = FirstLong(comps, "postal_code");

        // País
        var pais = FirstLong(comps, "country");

        // Latitude / Longitude
        var location = result.Geometry?.Location;

        return new EnderecoDto
        {
            Rua = rua,
            Numero = numero,
            Bairro = bairro,
            Cidade = cidade,
            Estado = estado,
            Cep = cep,
            Pais = pais,
            EnderecoFormatado = result.FormattedAddress ?? string.Empty,
            Lat = location?.Lat.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            Long = location?.Lng.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty
        };
    }
}
