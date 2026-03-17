using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using WSIntegration.Tests.Helpers;
using WSIntegration.Worker.Services;
using Xunit;

namespace WSIntegration.Tests;

/// <summary>
/// Test unitari di CountryInfoSoapService.
/// HttpClient è mockato: nessuna chiamata di rete reale.
/// </summary>
public sealed class CountryInfoSoapServiceTests
{
    // -----------------------------------------------------------------------
    // Factory helper
    // -----------------------------------------------------------------------

    private static CountryInfoSoapService CreateService(
        string responseBody,
        HttpStatusCode statusCode = HttpStatusCode.OK,
        out FakeHttpMessageHandler handler)
    {
        handler = new FakeHttpMessageHandler(responseBody, statusCode);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://fake-soap-endpoint/service")
        };
        return new CountryInfoSoapService(httpClient, NullLogger<CountryInfoSoapService>.Instance);
    }

    // -----------------------------------------------------------------------
    // ListCountriesByNameAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ListCountriesByName_ReturnsCorrectCount()
    {
        var svc = CreateService(SoapResponses.ListCountriesByName, out _);

        var result = await svc.ListCountriesByNameAsync();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task ListCountriesByName_ParsesIsoCodeAndName()
    {
        var svc = CreateService(SoapResponses.ListCountriesByName, out _);

        var result = await svc.ListCountriesByNameAsync();

        Assert.Contains(result, c => c.IsoCode == "IT" && c.Name == "Italy");
        Assert.Contains(result, c => c.IsoCode == "DE" && c.Name == "Germany");
        Assert.Contains(result, c => c.IsoCode == "US" && c.Name == "United States");
    }

    [Fact]
    public async Task ListCountriesByName_SendsCorrectSoapAction()
    {
        var svc = CreateService(SoapResponses.ListCountriesByName, out var handler);

        await svc.ListCountriesByNameAsync();

        // Verifica che il SOAPAction header sia corretto
        Assert.NotNull(handler.LastRequest);
        var soapAction = handler.LastRequest!.Content!.Headers.TryGetValues("SOAPAction", out var values)
            ? values.First() : null;
        Assert.Contains("ListOfCountryNamesByName", soapAction);
    }

    [Fact]
    public async Task ListCountriesByName_SendsPostMethod()
    {
        var svc = CreateService(SoapResponses.ListCountriesByName, out var handler);

        await svc.ListCountriesByNameAsync();

        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
    }

    [Fact]
    public async Task ListCountriesByName_RequestBodyContainsSoapEnvelope()
    {
        var svc = CreateService(SoapResponses.ListCountriesByName, out var handler);

        await svc.ListCountriesByNameAsync();

        Assert.Contains("Envelope", handler.LastRequestBody);
        Assert.Contains("ListOfCountryNamesByName", handler.LastRequestBody);
    }

    // -----------------------------------------------------------------------
    // GetFullCountryInfoAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetFullCountryInfo_ReturnsCorrectData()
    {
        var svc = CreateService(SoapResponses.FullCountryInfoItaly, out _);

        var result = await svc.GetFullCountryInfoAsync("IT");

        Assert.NotNull(result);
        Assert.Equal("IT",    result!.IsoCode);
        Assert.Equal("Italy", result.Name);
        Assert.Equal("Rome",  result.Capital);
        Assert.Equal("EU",    result.ContinentCode);
        Assert.Equal("EUR",   result.CurrencyIsoCode);
        Assert.Equal("Euro",  result.CurrencyName);
        Assert.Equal("39",    result.PhoneCode);
        Assert.Equal("380",   result.IsoNumeric);
    }

    [Fact]
    public async Task GetFullCountryInfo_ParsesLanguages()
    {
        var svc = CreateService(SoapResponses.FullCountryInfoItaly, out _);

        var result = await svc.GetFullCountryInfoAsync("IT");

        Assert.Contains("Italian", result!.Languages);
    }

    [Fact]
    public async Task GetFullCountryInfo_SendsIsoCodeInRequestBody()
    {
        var svc = CreateService(SoapResponses.FullCountryInfoItaly, out var handler);

        await svc.GetFullCountryInfoAsync("IT");

        Assert.Contains("IT", handler.LastRequestBody);
        Assert.Contains("sCountryISOCode", handler.LastRequestBody);
    }

    // -----------------------------------------------------------------------
    // Gestione errori
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ListCountriesByName_ThrowsOnSoapFault()
    {
        var svc = CreateService(SoapResponses.SoapFault, out _);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.ListCountriesByNameAsync());

        Assert.Contains("SOAP Fault", ex.Message);
        Assert.Contains("Invalid ISO code", ex.Message);
    }

    [Fact]
    public async Task GetFullCountryInfo_ThrowsOnHttpError()
    {
        var svc = CreateService("<error/>", HttpStatusCode.InternalServerError, out _);

        await Assert.ThrowsAsync<HttpRequestException>(
            () => svc.GetFullCountryInfoAsync("XX"));
    }

    [Fact]
    public async Task GetFullCountryInfo_ReturnsNullWhenResultMissing()
    {
        // Risposta valida ma senza FullCountryInfoResult
        const string emptyResponse = """
            <?xml version="1.0" encoding="utf-8"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <FullCountryInfoResponse xmlns="http://www.oorsprong.org/websamples.countryinfo">
                </FullCountryInfoResponse>
              </soap:Body>
            </soap:Envelope>
            """;

        var svc = CreateService(emptyResponse, out _);

        var result = await svc.GetFullCountryInfoAsync("ZZ");

        Assert.Null(result);
    }

    [Fact]
    public async Task ListCountriesByName_ThrowsOnInvalidXml()
    {
        var svc = CreateService("questo non è XML valido!!!", out _);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.ListCountriesByNameAsync());
    }
}
