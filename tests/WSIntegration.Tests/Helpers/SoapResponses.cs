namespace WSIntegration.Tests.Helpers;

/// <summary>
/// Risposte SOAP campione usate nei test unitari.
/// Simulate da risposte reali del WS CountryInfo.
/// </summary>
internal static class SoapResponses
{
    public const string ListCountriesByName = """
        <?xml version="1.0" encoding="utf-8"?>
        <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
          <soap:Body>
            <ListOfCountryNamesByNameResponse xmlns="http://www.oorsprong.org/websamples.countryinfo">
              <ListOfCountryNamesByNameResult>
                <tCountryCodeAndName>
                  <sISOCode>DE</sISOCode>
                  <sName>Germany</sName>
                </tCountryCodeAndName>
                <tCountryCodeAndName>
                  <sISOCode>IT</sISOCode>
                  <sName>Italy</sName>
                </tCountryCodeAndName>
                <tCountryCodeAndName>
                  <sISOCode>US</sISOCode>
                  <sName>United States</sName>
                </tCountryCodeAndName>
              </ListOfCountryNamesByNameResult>
            </ListOfCountryNamesByNameResponse>
          </soap:Body>
        </soap:Envelope>
        """;

    public const string FullCountryInfoItaly = """
        <?xml version="1.0" encoding="utf-8"?>
        <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
          <soap:Body>
            <FullCountryInfoResponse xmlns="http://www.oorsprong.org/websamples.countryinfo">
              <FullCountryInfoResult>
                <sISOCode>IT</sISOCode>
                <sName>Italy</sName>
                <sISONumeric>380</sISONumeric>
                <sCountryFlag>http://webservices.oorsprong.org/websamples.countryinfo/Flags/Italy.jpg</sCountryFlag>
                <sCapital>Rome</sCapital>
                <sContinentCode>EU</sContinentCode>
                <sCurrencyISOCode>EUR</sCurrencyISOCode>
                <sCurrencyName>Euro</sCurrencyName>
                <sPhoneCode>39</sPhoneCode>
                <Languages>
                  <tLanguage>
                    <sISOCode>it</sISOCode>
                    <sName>Italian</sName>
                  </tLanguage>
                </Languages>
              </FullCountryInfoResult>
            </FullCountryInfoResponse>
          </soap:Body>
        </soap:Envelope>
        """;

    public const string SoapFault = """
        <?xml version="1.0" encoding="utf-8"?>
        <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
          <soap:Body>
            <soap:Fault>
              <faultcode>soap:Client</faultcode>
              <faultstring>Invalid ISO code</faultstring>
            </soap:Fault>
          </soap:Body>
        </soap:Envelope>
        """;
}
