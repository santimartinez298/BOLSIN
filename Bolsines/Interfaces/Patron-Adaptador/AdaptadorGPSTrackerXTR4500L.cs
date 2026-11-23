using System.Globalization;
using System.Text.Json.Nodes;
using Bolsines.Interfaces.Dispositivos;

namespace Bolsines.Interfaces.Patron_Adaptador
{
    // ---------------------------------------------------------------------
    // ADAPTER del patrón Adapter.
    //
    // Implementa la interfaz que espera el sistema (IAdaptadorGPSTracker) y
    // traduce la respuesta del GPS XTR-4500L (JSON con un arreglo "items")
    // al formato que usa el sistema: String[][] (ver IAdaptadorGPSTracker).
    // ---------------------------------------------------------------------
    public class AdaptadorGPSTrackerXTR4500L : IAdaptadorGPSTracker
    {
        // Atributos
        private string apiKey;
        private int[] numeroBolsin;
        private string codigoCMOrigen;
        private string localizacionRecibida = "";
        private string[][] localizacionAdaptada = new string[0][];

        // Agregación con el Adaptee (ServicioGPSTrackerXTR4500L)
        private ServicioGPSTrackerXTR4500L gpsXTR4500L;

        // Constructor: new(apiKey, numeroBolsin, codigoCMOrigen)
        public AdaptadorGPSTrackerXTR4500L(string apiKey, int[] numeroBolsin, string codigoCMOrigen)
        {
            this.apiKey = apiKey;
            this.numeroBolsin = numeroBolsin;
            this.codigoCMOrigen = codigoCMOrigen;
            this.gpsXTR4500L = new ServicioGPSTrackerXTR4500L();
        }

        // Métodos
        public string[][] recibirUbicacionBolsin()
        {
            this.solicitarUbicacionBolsin();
            this.adaptarUbicacionBolsin();
            return this.localizacionAdaptada;
        }

        // Le pide al GPS (Adaptee) la ubicación de los bolsines, usando la
        // interfaz propia del dispositivo. Las respuestas se juntan en
        // localizacionRecibida para adaptarlas todas juntas después del loop.
        public void solicitarUbicacionBolsin()
        {
            JsonArray itemsRecibidos = new JsonArray();

            // loop [Mientras haya Bolsines por analizar]
            foreach (int numero in this.numeroBolsin)
            {
                string respuesta = this.gpsXTR4500L.getBolsinLocation(
                    this.apiKey,
                    new int[] { numero },
                    this.codigoCMOrigen
                );

                JsonArray? items = JsonNode.Parse(respuesta)?["items"]?.AsArray();

                if (items is not null)
                {
                    foreach (JsonNode? item in items)
                    {
                        if (item is not null)
                        {
                            itemsRecibidos.Add(item.DeepClone());
                        }
                    }
                }
            }

            this.localizacionRecibida = new JsonObject { ["items"] = itemsRecibidos }.ToJsonString();
        }

        // Traduce el JSON del XTR-4500L al formato que usa el sistema (String[][]).
        public void adaptarUbicacionBolsin()
        {
            List<string[]> filas = new List<string[]>();
            JsonArray? items = JsonNode.Parse(this.localizacionRecibida)?["items"]?.AsArray();

            if (items is not null)
            {
                foreach (JsonNode? item in items)
                {
                    if (item is null)
                    {
                        continue;
                    }

                    // 1. Extraer número y coordenadas
                    int numero = item["numeroBolsin"]!.GetValue<int>();
                    double latitud = item["latitud"]!.GetValue<double>();
                    double longitud = item["longitud"]!.GetValue<double>();

                    // 2. Extraer fecha y hora. La API la entrega como texto ISO sin
                    //    zona horaria ("2026-03-07T12:30:00"), en hora local.
                    DateTime fechaHora = DateTime.Parse(
                        item["fechaHoraUltimaActualizacion"]!.GetValue<string>(),
                        CultureInfo.InvariantCulture
                    );

                    // 3. Armar la fila en el formato del sistema
                    filas.Add(
                        new string[]
                        {
                            numero.ToString(CultureInfo.InvariantCulture),
                            latitud.ToString("R", CultureInfo.InvariantCulture),
                            longitud.ToString("R", CultureInfo.InvariantCulture),
                            fechaHora.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
                        }
                    );
                }
            }

            this.localizacionAdaptada = filas.ToArray();
        }
    }
}
