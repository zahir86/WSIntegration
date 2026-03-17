namespace WSIntegration.Worker.Models;

/// <summary>
/// Rappresenta il nome di una nazione (ISO code + nome).
/// </summary>
public record CountryName(string IsoCode, string Name);

/// <summary>
/// Informazioni complete di una nazione restituite dal WS CountryInfo.
/// </summary>
public record CountryFullInfo(
    string IsoCode,
    string Name,
    string IsoNumeric,
    string CountryFlag,
    string Capital,
    string ContinentCode,
    string CurrencyIsoCode,
    string CurrencyName,
    string PhoneCode,
    string Languages
);
