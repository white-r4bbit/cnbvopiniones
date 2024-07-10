using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Cnbv.ConectaProcesos.Opiniones.Common
{
  public class AppInsightsService
  {
    private readonly TelemetryConfiguration _telemetryClient;
    private readonly IConfiguration _config;

    public AppInsightsService(IOptions<TelemetryConfiguration> telemetryClient, IConfiguration config)
    {
      _telemetryClient = telemetryClient.Value;
      _config = config;
    }

    private TelemetryClient GetTelemetryClient()
    {
      return new TelemetryClient(_telemetryClient);
    }

    public void TrackEvent(string name, Dictionary<string, string> properties, string clientIPAddress)
    {
      properties["ClientIPAddress"] = clientIPAddress;
      GetTelemetryClient().TrackEvent(name, properties);
    }

    public void TrackException(Exception ex, string clientIPAddress)
    {
      var properties = new Dictionary<string, string>
      {
        ["ClientIPAddress"] = clientIPAddress // Agregar la IP como propiedad personalizada
      };

      GetTelemetryClient().TrackException(ex, properties);
    }
  }
}
