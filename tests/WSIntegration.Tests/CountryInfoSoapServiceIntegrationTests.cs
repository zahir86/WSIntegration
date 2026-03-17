using Microsoft.Extensions.Logging.Abstractions;
using WSIntegration.Worker.Services;
using Xunit;

namespace WSIntegration.Tests;

/// <summary>
/// Test di integrazione: chiamano il WS SOAP reale su internet.
/// Decorati con [Trait("Category","Integration")] per escluderli dalla CI normale.
///
/// Esecuzione selettiva:
///   dotnet test --filter "Category=Integration"
///
/// Prerequisito: connessione internet attiva.
/// </summary>
[Trait("Category", "Integration")]
public sealed class CountryInfoSoapServiceIntegrationTests
{
    private static CountryInfoSoapService CreateRealService()
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://webservices.oorsprong.org/websamples.countryinfo/CountryInfoService.wso"),
            Timeout     = TimeSpan.FromSeconds(30)
        };
        return new CountryInfoSoapService(httpClient, NullLogger<CountryInfoSoapService>.Instance);
    }

    [Fact]
    public async Task ListCountriesByName_ReturnsNonEmptyList_Real()
    {
        var svc = CreateRealService();

        var countries = await svc.ListCountriesByNameAsync();

        Assert.NotEmpty(countries);
        // Il WS restituisce ~246 nazioni
        Assert.True(countries.Count > 100, $"Attese >100 nazioni, ricevute: {countries.Count}");
        Assert.All(countries, c =>
        {
            Assert.False(string.IsNullOrWhiteSpace(c.IsoCode));
            Assert.False(string.IsNullOrWhiteSpace(c.Name));
        });
    }

    [Fact]
    public async Task GetFullCountryInfo_Italy_ReturnsCorrectData_Real()
    {
        var svc = CreateRealService();

        var info = await svc.GetFullCountryInfoAsync("IT");

        Assert.NotNull(info);
        Assert.Equal("IT",    info!.IsoCode);
        Assert.Equal("Italy", info.Name);
        Assert.Equal("Rome",  info.Capital);
        Assert.Equal("EU",    info.ContinentCode);
        Assert.Equal("EUR",   info.CurrencyIsoCode);
        Assert.Equal("39",    info.PhoneCode);
        Assert.Contains("Italian", info.Languages);
    }

    [Fact]
    public async Task GetFullCountryInfo_USA_ReturnsCorrectData_Real()
    {
        var svc = CreateRealService();

        var info = await svc.GetFullCountryInfoAsync("US");

        Assert.NotNull(info);
        Assert.Equal("US",              info!.IsoCode);
        Assert.Equal("United States",   info.Name);
        Assert.Equal("Washington",      info.Capital);
        Assert.Equal("NA",              info.ContinentCode);
        Assert.Equal("USD",             info.CurrencyIsoCode);
        Assert.Equal("1",               info.PhoneCode);
    }
}
