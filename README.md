# WSIntegration — SOAP Worker Service

Esempio di **NET 8 Worker Service** che consuma un WS SOAP pubblico usando `HttpClient` (niente WCF, niente `dotnet-svcutil`).

## WS SOAP utilizzato

| Proprietà | Valore |
|-----------|--------|
| Nome      | CountryInfo Service |
| Endpoint  | `http://webservices.oorsprong.org/websamples.countryinfo/CountryInfoService.wso` |
| WSDL      | [Link](http://webservices.oorsprong.org/websamples.countryinfo/CountryInfoService.wso?WSDL) |
| Protocollo | SOAP 1.1 |

## Struttura progetto

```
WSIntegration/
├── WSIntegration.sln
└── src/
    └── WSIntegration.Worker/
        ├── Program.cs                         # Bootstrap DI + HttpClient
        ├── Worker.cs                          # BackgroundService (loop di polling)
        ├── appsettings.json                   # Configurazione
        ├── Configuration/
        │   ├── WorkerOptions.cs               # Opzioni Worker (intervallo, paese campione)
        │   └── SoapServiceOptions.cs          # URL + timeout del WS
        ├── Models/
        │   └── CountryInfo.cs                 # Record CountryName, CountryFullInfo
        └── Services/
            ├── ICountryInfoSoapService.cs     # Interfaccia
            └── CountryInfoSoapService.cs      # Implementazione SOAP via HttpClient
```

## Come funziona

```
┌─────────────────────┐   ogni N secondi   ┌──────────────────────────┐
│     Worker.cs       │ ─────────────────► │ ICountryInfoSoapService  │
│  BackgroundService  │                    │ CountryInfoSoapService    │
└─────────────────────┘                    └──────────┬───────────────┘
                                                      │ HttpClient
                                                      │ POST + SOAP envelope XML
                                                      ▼
                                           ┌──────────────────────────┐
                                           │  CountryInfo SOAP WS     │
                                           │  (endpoint pubblico)     │
                                           └──────────────────────────┘
```

Il ciclo ad ogni tick:
1. Chiama `ListOfCountryNamesByName` → stampa le prime N nazioni
2. Chiama `FullCountryInfo` per la nazione configurata (default `IT`) → stampa dettagli

## Avvio rapido

```bash
cd src/WSIntegration.Worker
dotnet run
```

## Configurazione (`appsettings.json`)

```json
{
  "Worker": {
    "PollingIntervalSeconds": 60,   // intervallo tra i cicli
    "SampleCountryIsoCode": "IT",   // ISO-3166-1 alpha-2 della nazione campione
    "PreviewCount": 5               // quante nazioni mostrare nella lista
  },
  "SoapServices": {
    "CountryInfo": {
      "BaseUrl": "http://webservices.oorsprong.org/...",
      "TimeoutSeconds": 30
    }
  }
}
```

## Adattare ad un altro WS SOAP

1. Creare una nuova interfaccia in `Services/` (es. `IMyWsSoapService`)
2. Implementarla estendendo il pattern in `CountryInfoSoapService` (metodi `BuildEnvelope`, `SendAsync`, `ParseBody`)
3. Registrare l'`HttpClient` in `Program.cs`
4. Aggiungere le opzioni in `appsettings.json` sotto `SoapServices`
