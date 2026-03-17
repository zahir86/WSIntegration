namespace WSIntegration.Worker.Configuration;

/// <summary>
/// Opzioni di connessione per un singolo WS SOAP, lette da appsettings.json.
/// </summary>
public sealed class SoapServiceOptions
{
    /// <summary>URL base dell'endpoint SOAP (senza ?WSDL).</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>Timeout delle richieste HTTP in secondi.</summary>
    public int TimeoutSeconds { get; set; } = 30;
}
