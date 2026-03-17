namespace WSIntegration.Worker.Configuration;

/// <summary>
/// Opzioni del Worker lette da appsettings.json sezione "Worker".
/// </summary>
public sealed class WorkerOptions
{
    public const string Section = "Worker";

    /// <summary>Intervallo in secondi tra un ciclo di polling e il successivo.</summary>
    public int PollingIntervalSeconds { get; set; } = 30;

    /// <summary>Codice ISO-3166-1 alpha-2 della nazione da usare come campione (es. "IT").</summary>
    public string SampleCountryIsoCode { get; set; } = "IT";

    /// <summary>Numero di nazioni da mostrare in anteprima nella lista.</summary>
    public int PreviewCount { get; set; } = 5;
}
