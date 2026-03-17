using WSIntegration.Worker.Models;

namespace WSIntegration.Worker.Services;

/// <summary>
/// Contratto per il client SOAP del WS CountryInfo.
/// Endpoint pubblico: http://webservices.oorsprong.org/websamples.countryinfo/CountryInfoService.wso
/// WSDL:             http://webservices.oorsprong.org/websamples.countryinfo/CountryInfoService.wso?WSDL
/// </summary>
public interface ICountryInfoSoapService
{
    /// <summary>Restituisce la lista di tutte le nazioni ordinate per nome.</summary>
    Task<IReadOnlyList<CountryName>> ListCountriesByNameAsync(CancellationToken ct = default);

    /// <summary>Restituisce i dettagli completi di una nazione dato il codice ISO-3166-1 alpha-2 (es. "IT", "US").</summary>
    Task<CountryFullInfo?> GetFullCountryInfoAsync(string isoCode, CancellationToken ct = default);
}
