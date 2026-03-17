using System.Net;

namespace WSIntegration.Tests.Helpers;

/// <summary>
/// HttpMessageHandler che restituisce una risposta predefinita.
/// Permette di testare CountryInfoSoapService senza chiamate di rete reali.
/// </summary>
public sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _responseBody;

    /// <summary>Ultima richiesta ricevuta (utile per assert sul payload SOAP inviato).</summary>
    public HttpRequestMessage? LastRequest { get; private set; }
    public string? LastRequestBody { get; private set; }

    public FakeHttpMessageHandler(string responseBody, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _responseBody = responseBody;
        _statusCode   = statusCode;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest     = request;
        LastRequestBody = request.Content is not null
            ? await request.Content.ReadAsStringAsync(cancellationToken)
            : null;

        return new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_responseBody, System.Text.Encoding.UTF8, "text/xml")
        };
    }
}
