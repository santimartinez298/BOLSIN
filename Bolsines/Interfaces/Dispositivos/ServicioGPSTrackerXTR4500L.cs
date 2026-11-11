namespace Bolsines.Interfaces.Dispositivos
{
    // ---------------------------------------------------------------------
    // ADAPTEE del patrón Adapter: GPS Tracker modelo XTR-4500L.
    //
    // Ya no simula la ubicación: la pide a la API provista por la cátedra
    // (repositorio dsi-api-bolsines), que debe estar corriendo en
    // http://localhost:8080.
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
        // Devuelve el JSON tal cual lo entrega el dispositivo, o null si no
        // hay ubicación para ese bolsín (401, 400, 404) o si la API no responde.
        public string? getBolsinLocation(string apiKey, int numeroBolsin, string codigoCMOrigen)
        {
            string url =
                "/api/xtr-4500l/getBolsinLocation"
                + $"?numeroBolsin={numeroBolsin}"
                + $"&codigoComisionMedicaOrigen={Uri.EscapeDataString(codigoCMOrigen)}";

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
                        $"GPS XTR-4500L: el bolsín {numeroBolsin} no tiene ubicación "
                            + $"(HTTP {(int)respuesta.StatusCode}): {cuerpo}"
                    );
                    return null;
                }

                return cuerpo;
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
            {
                Console.WriteLine(
                    $"GPS XTR-4500L: no se pudo conectar con la API en {urlBaseAPI}. "
                        + $"¿Está levantada? ({ex.Message})"
                );
                return null;
            }
        }
    }
}
