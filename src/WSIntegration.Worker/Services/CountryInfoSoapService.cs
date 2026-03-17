using System.Net.Http.Headers;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WSIntegration.Worker.Models;

namespace WSIntegration.Worker.Services;

/// <summary>
/// Implementazione del client SOAP per il WS pubblico CountryInfo.
/// Utilizza HttpClient per inviare buste SOAP 1.1 e XLinq per il parsing della risposta.
/// </summary>
public sealed class CountryInfoSoapService : ICountryInfoSoapService
{
    // Namespace XML usati dal servizio
    private static readonly XNamespace SoapNs   = "http://schemas.xmlsoap.org/soap/envelope/";
    private static readonly XNamespace ServiceNs = "http://www.oorsprong.org/websamples.countryinfo";

    private readonly HttpClient _http;
    private readonly ILogger<CountryInfoSoapService> _logger;

    public CountryInfoSoapService(HttpClient http, ILogger<CountryInfoSoapService> logger)
    {
        _http   = http;
        _logger = logger;
    }

    // -------------------------------------------------------------------------
    // ListOfCountryNamesByName
    // -------------------------------------------------------------------------

    public async Task<IReadOnlyList<CountryName>> ListCountriesByNameAsync(CancellationToken ct = default)
    {
        const string action = "ListOfCountryNamesByName";

        var envelope = BuildEnvelope(
            new XElement(ServiceNs + action)
        );

        var response = await SendAsync(envelope, action, ct);
        var body     = ParseBody(response);

        // Struttura attesa:
        // <ListOfCountryNamesByNameResponse>
        //   <ListOfCountryNamesByNameResult>
        //     <tCountryCodeAndName>
        //       <sISOCode>IT</sISOCode>
        //       <sName>Italy</sName>
        //     </tCountryCodeAndName>
        //     ...
        //   </ListOfCountryNamesByNameResult>
        // </ListOfCountryNamesByNameResponse>

        var items = body
            .Descendants(ServiceNs + "tCountryCodeAndName")
            .Select(e => new CountryName(
                IsoCode: (string?)e.Element(ServiceNs + "sISOCode") ?? string.Empty,
                Name:    (string?)e.Element(ServiceNs + "sName")    ?? string.Empty
            ))
            .ToList();

        _logger.LogDebug("ListCountriesByNameAsync returned {Count} countries.", items.Count);
        return items;
    }

    // -------------------------------------------------------------------------
    // FullCountryInfo
    // -------------------------------------------------------------------------

    public async Task<CountryFullInfo?> GetFullCountryInfoAsync(string isoCode, CancellationToken ct = default)
    {
        const string action = "FullCountryInfo";

        var envelope = BuildEnvelope(
            new XElement(ServiceNs + action,
                new XElement(ServiceNs + "sCountryISOCode", isoCode)
            )
        );

        var response = await SendAsync(envelope, action, ct);
        var body     = ParseBody(response);

        // Struttura attesa:
        // <FullCountryInfoResponse>
        //   <FullCountryInfoResult>
        //     <sISOCode>IT</sISOCode>
        //     <sName>Italy</sName>
        //     <sISONumeric>380</sISONumeric>
        //     <sCountryFlag>http://...</sCountryFlag>
        //     <sCapital>Rome</sCapital>
        //     <sContinentCode>EU</sContinentCode>
        //     <sCurrencyISOCode>EUR</sCurrencyISOCode>
        //     <sCurrencyName>Euro</sCurrencyName>
        //     <sPhoneCode>39</sPhoneCode>
        //     <Languages>...</Languages>
        //   </FullCountryInfoResult>
        // </FullCountryInfoResponse>

        var result = body.Descendants(ServiceNs + "FullCountryInfoResult").FirstOrDefault();
        if (result is null)
        {
            _logger.LogWarning("FullCountryInfo: nessun risultato per isoCode={IsoCode}", isoCode);
            return null;
        }

        string E(string name) => (string?)result.Element(ServiceNs + name) ?? string.Empty;

        // Le lingue sono in elementi annidati <Languages><tLanguage><sISOCode>it</sISOCode><sName>Italian</sName></tLanguage></Languages>
        var languages = string.Join(", ",
            result.Descendants(ServiceNs + "tLanguage")
                  .Select(l => (string?)l.Element(ServiceNs + "sName") ?? string.Empty)
                  .Where(s => s.Length > 0));

        return new CountryFullInfo(
            IsoCode:          E("sISOCode"),
            Name:             E("sName"),
            IsoNumeric:       E("sISONumeric"),
            CountryFlag:      E("sCountryFlag"),
            Capital:          E("sCapital"),
            ContinentCode:    E("sContinentCode"),
            CurrencyIsoCode:  E("sCurrencyISOCode"),
            CurrencyName:     E("sCurrencyName"),
            PhoneCode:        E("sPhoneCode"),
            Languages:        languages
        );
    }

    // -------------------------------------------------------------------------
    // Helpers privati
    // -------------------------------------------------------------------------

    /// <summary>Costruisce una busta SOAP 1.1 con il body indicato.</summary>
    private static XDocument BuildEnvelope(XElement bodyContent) =>
        new(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(SoapNs + "Envelope",
                new XAttribute(XNamespace.Xmlns + "soap", SoapNs),
                new XAttribute(XNamespace.Xmlns + "ns",   ServiceNs),
                new XElement(SoapNs + "Body", bodyContent)
            )
        );

    /// <summary>Invia la busta SOAP e restituisce la risposta come stringa XML.</summary>
    private async Task<string> SendAsync(XDocument envelope, string soapAction, CancellationToken ct)
    {
        var xml     = envelope.ToString(SaveOptions.DisableFormatting);
        var content = new StringContent(xml, Encoding.UTF8, "text/xml");

        // SOAPAction header richiesto da SOAP 1.1
        content.Headers.Add("SOAPAction",
            $"\"http://www.oorsprong.org/websamples.countryinfo/{soapAction}\"");

        _logger.LogDebug("SOAP Request [{Action}]:\n{Xml}", soapAction, xml);

        var httpResponse = await _http.PostAsync(string.Empty, content, ct);
        var responseXml  = await httpResponse.Content.ReadAsStringAsync(ct);

        _logger.LogDebug("SOAP Response [{Action}] HTTP {Status}:\n{Xml}",
            soapAction, httpResponse.StatusCode, responseXml);

        if (!httpResponse.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"SOAP call {soapAction} failed with HTTP {(int)httpResponse.StatusCode}: {responseXml}");

        return responseXml;
    }

    /// <summary>Parsa la risposta e restituisce il nodo &lt;Body&gt; della busta.</summary>
    private XElement ParseBody(string responseXml)
    {
        XDocument doc;
        try
        {
            doc = XDocument.Parse(responseXml);
        }
        catch (XmlException ex)
        {
            throw new InvalidOperationException($"Risposta XML non valida: {ex.Message}", ex);
        }

        var body = doc.Root?.Element(SoapNs + "Body");
        if (body is null)
            throw new InvalidOperationException("Risposta SOAP priva di elemento <Body>.");

        // Controlla eventuali Fault
        var fault = body.Element(SoapNs + "Fault");
        if (fault is not null)
        {
            var faultCode   = (string?)fault.Element("faultcode")   ?? "N/A";
            var faultString = (string?)fault.Element("faultstring") ?? "N/A";
            throw new InvalidOperationException($"SOAP Fault [{faultCode}]: {faultString}");
        }

        return body;
    }
}
