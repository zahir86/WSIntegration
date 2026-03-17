using Microsoft.Extensions.Options;
using WSIntegration.Worker.Services;
using WSIntegration.Worker.Configuration;

namespace WSIntegration.Worker;

/// <summary>
/// Worker Service che interroga periodicamente il WS SOAP CountryInfo.
/// Ad ogni ciclo:
///   1. Recupera la lista completa delle nazioni
///   2. Stampa i dettagli di una nazione campione (configurabile)
/// </summary>
public sealed class Worker : BackgroundService
{
    private readonly ICountryInfoSoapService _soapService;
    private readonly WorkerOptions _options;
    private readonly ILogger<Worker> _logger;

    public Worker(
        ICountryInfoSoapService soapService,
        IOptions<WorkerOptions> options,
        ILogger<Worker> logger)
    {
        _soapService = soapService;
        _options     = options.Value;
        _logger      = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Worker avviato. Intervallo: {Interval}s | Nazione campione: {Country}",
            _options.PollingIntervalSeconds,
            _options.SampleCountryIsoCode);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunCycleAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il ciclo di polling SOAP.");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(_options.PollingIntervalSeconds),
                stoppingToken);
        }

        _logger.LogInformation("Worker terminato.");
    }

    private async Task RunCycleAsync(CancellationToken ct)
    {
        _logger.LogInformation("=== Inizio ciclo SOAP [{Time}] ===", DateTimeOffset.Now);

        // 1. Lista nazioni
        var countries = await _soapService.ListCountriesByNameAsync(ct);
        _logger.LogInformation("Totale nazioni disponibili: {Count}", countries.Count);

        if (countries.Count > 0)
        {
            // Mostra le prime N nazioni
            var preview = countries.Take(_options.PreviewCount);
            _logger.LogInformation("Prime {N} nazioni:", _options.PreviewCount);
            foreach (var c in preview)
                _logger.LogInformation("  [{Iso}] {Name}", c.IsoCode, c.Name);
        }

        // 2. Dettagli nazione campione
        _logger.LogInformation("Recupero dettagli per: {IsoCode}", _options.SampleCountryIsoCode);
        var info = await _soapService.GetFullCountryInfoAsync(_options.SampleCountryIsoCode, ct);

        if (info is not null)
        {
            _logger.LogInformation(
                """
                --- Dettagli [{IsoCode}] ---
                  Nome:       {Name}
                  Capitale:   {Capital}
                  Continente: {Continent}
                  Valuta:     {Currency} ({CurrencyCode})
                  Tel. prefix: +{Phone}
                  Lingue:     {Languages}
                  Bandiera:   {Flag}
                """,
                info.IsoCode, info.Name, info.Capital,
                info.ContinentCode, info.CurrencyName, info.CurrencyIsoCode,
                info.PhoneCode, info.Languages, info.CountryFlag);
        }

        _logger.LogInformation("=== Fine ciclo SOAP ===");
    }
}
