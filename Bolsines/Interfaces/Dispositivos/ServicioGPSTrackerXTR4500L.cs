using System.Text.Json.Nodes;

namespace Bolsines.Interfaces.Dispositivos
{
    // ---------------------------------------------------------------------
    // ADAPTEE del patrón Adapter: GPS Tracker modelo XTR-4500L.
    //
    // Pide la ubicación a la API provista por la cátedra (repositorio
    // dsi-api-bolsines), que debe estar corriendo en http://localhost:8080.
    //
    // Interfaz propia del dispositivo (incompatible con la del sistema):
    //   GET /api/xtr-4500l/getBolsinLocation
    //       ?numeroBolsin={nro}&codigoComisionMedicaOrigen={codigo}
    //   Header: X-API-Key
    //   Respuesta: {"items":[{"numeroBolsin":101,"latitud":-31.42,
    //               "longitud":-64.18,"fechaHoraUltimaActualizacion":"2026-03-07T12:30:00"}]}
    // ---------------------------------------------------------------------
    public class ServicioGPSTrackerXTR4500L
    {
        // Atributos (no van en el diagrama de clases)
        private const string urlBaseAPI = "http://localhost:8080";

        private static readonly HttpClient clienteHttp = new HttpClient
        {
            BaseAddress = new Uri(urlBaseAPI),
            Timeout = TimeSpan.FromSeconds(5),
        };

        // Métodos
        // getBolsinLocation(apiKey: String, nroBolsin: int[], codCMOrigen: String): String
        // Devuelve un JSON {"items":[...]} con la ubicación de los bolsines que
        // el dispositivo pudo informar. Los que no (401, 400, 404 o API caída)
        // no aparecen en "items".
        public string getBolsinLocation(string apiKey, int[] nroBolsin, string codCMOrigen)
        {
            JsonArray items = new JsonArray();

            foreach (int numero in nroBolsin)
            {
                string url =
                    "/api/xtr-4500l/getBolsinLocation"
                    + $"?numeroBolsin={numero}"
                    + $"&codigoComisionMedicaOrigen={Uri.EscapeDataString(codCMOrigen)}";

                using var solicitud = new HttpRequestMessage(HttpMethod.Get, url);
                solicitud.Headers.Add("X-API-Key", apiKey);

                try
                {
                    using HttpResponseMessage respuesta = clienteHttp.Send(solicitud);
                    using var lector = new StreamReader(respuesta.Content.ReadAsStream());
                    string cuerpo = lector.ReadToEnd();

                    if (!respuesta.IsSuccessStatusCode)
                    {
                        Console.WriteLine(
                            $"GPS XTR-4500L: el bolsín {numero} no tiene ubicación "
                                + $"(HTTP {(int)respuesta.StatusCode}): {cuerpo}"
                        );
                        continue;
                    }

                    JsonArray? recibidos = JsonNode.Parse(cuerpo)?["items"]?.AsArray();

                    if (recibidos is not null)
                    {
                        foreach (JsonNode? item in recibidos)
                        {
                            if (item is not null)
                            {
                                items.Add(item.DeepClone());
                            }
                        }
                    }
                }
                catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
                {
                    Console.WriteLine(
                        $"GPS XTR-4500L: no se pudo conectar con la API en {urlBaseAPI}. "
                            + $"¿Está levantada? ({ex.Message})"
                    );
                    break;
                }
            }

            return new JsonObject { ["items"] = items }.ToJsonString();
        }
    }
}
