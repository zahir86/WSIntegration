using WSIntegration.Worker;
using WSIntegration.Worker.Configuration;
using WSIntegration.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);

// -------------------------------------------------------------------------
// Configurazione
// -------------------------------------------------------------------------
builder.Services.Configure<WorkerOptions>(
    builder.Configuration.GetSection(WorkerOptions.Section));

var countryInfoOptions = builder.Configuration
    .GetSection("SoapServices:CountryInfo")
    .Get<SoapServiceOptions>()
    ?? new SoapServiceOptions
    {
        BaseUrl        = "http://webservices.oorsprong.org/websamples.countryinfo/CountryInfoService.wso",
        TimeoutSeconds = 30
    };

// -------------------------------------------------------------------------
// HttpClient per il servizio SOAP CountryInfo
// (IHttpClientFactory gestisce pooling e lifetime)
// -------------------------------------------------------------------------
builder.Services
    .AddHttpClient<ICountryInfoSoapService, CountryInfoSoapService>(client =>
    {
        client.BaseAddress = new Uri(countryInfoOptions.BaseUrl);
        client.Timeout     = TimeSpan.FromSeconds(countryInfoOptions.TimeoutSeconds);
        client.DefaultRequestHeaders.Add("Accept", "text/xml");
    });

// -------------------------------------------------------------------------
// Worker
// -------------------------------------------------------------------------
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
