using System.Globalization;
using System.Text.Json;
using Bolsines.Entidades;
using Bolsines.Interfaces.Dispositivos;

namespace Bolsines.Interfaces.Patron_Adaptador
{
    // ---------------------------------------------------------------------
    // ADAPTER del patrón Adapter.
    //
    // Implementa la interfaz que espera el sistema (IAdaptadorGPSTracker) y
    // traduce la respuesta del GPS XTR-4500L (JSON con un arreglo "items")
    // a la lista de tuplas que usa el controlador:
    //   (Bolsin, numeroBolsin, (latitud, longitud), fechaHoraUbicacion)
    // ---------------------------------------------------------------------
    public class AdaptadorGPSTrackerXTR4500L : IAdaptadorGPSTracker
    {
        // Atributos
        private string apiKey;
        private List<(Bolsin bolsin, int numero)> bolsinesEnviados;
        private string codigoCMOrigen;
        private string? localizacionRecibida;
        private List<(
            Bolsin bolsin,
            int numeroBolsin,
            (double latitud, double longitud) ubicacion,
            DateTime fechaHoraUbicacion
        )> localizacionAdaptada = new();
        private ServicioGPSTrackerXTR4500L gpsXTR4500L;

        // Constructor
        public AdaptadorGPSTrackerXTR4500L(
            string apiKey,
            List<(Bolsin bolsin, int numero)> bolsinesEnviados,
            string codigoCMOrigen
        )
        {
            this.apiKey = apiKey;
            this.bolsinesEnviados = bolsinesEnviados;
            this.codigoCMOrigen = codigoCMOrigen;
            this.gpsXTR4500L = new ServicioGPSTrackerXTR4500L();
        }

        // Métodos
        public List<(
            Bolsin bolsin,
            int numeroBolsin,
            (double latitud, double longitud) ubicacion,
            DateTime fechaHoraUbicacion
        )> recibirUbicacionBolsin()
        {
            this.solicitarUbicacionBolsin();
            return this.localizacionAdaptada;
        }

        // Le pide al GPS (Adaptee) la ubicación de cada bolsín enviado,
        // usando la interfaz propia del dispositivo.
        public void solicitarUbicacionBolsin()
        {
            foreach (var bolsin in this.bolsinesEnviados)
            {
                var localizacion = this.gpsXTR4500L.getBolsinLocation(
                    this.apiKey,
                    bolsin.numero,
                    this.codigoCMOrigen
                );

                if (localizacion is not null)
                {
                    this.localizacionRecibida = localizacion;
                    this.adaptarUbicacionBolsin();
                }
            }
        }

        // Traduce el JSON del XTR-4500L al formato que usa el sistema.
        public void adaptarUbicacionBolsin()
        {
            if (this.localizacionRecibida == null) return;

            // 1. Parsear el JSON: {"items": [ {...}, ... ]}
            using JsonDocument doc = JsonDocument.Parse(this.localizacionRecibida);
            JsonElement items = doc.RootElement.GetProperty("items");

            foreach (JsonElement item in items.EnumerateArray())
            {
                // 2. Extraer número y coordenadas
                int numeroBolsin = item.GetProperty("numeroBolsin").GetInt32();
                double latitud = item.GetProperty("latitud").GetDouble();
                double longitud = item.GetProperty("longitud").GetDouble();

                // 3. Extraer fecha y hora. La API la entrega como texto ISO sin
                //    zona horaria ("2026-03-07T12:30:00"), en hora local.
                DateTime fechaHoraUltimaActualizacion = DateTime.Parse(
                    item.GetProperty("fechaHoraUltimaActualizacion").GetString()!,
                    CultureInfo.InvariantCulture
                );

                // 4. Asociar la ubicación con el objeto Bolsin correspondiente
                foreach (var bolsin in this.bolsinesEnviados)
                {
                    if (bolsin.numero == numeroBolsin)
                    {
                        this.localizacionAdaptada.Add(
                            (
                                bolsin.bolsin,
                                numeroBolsin,
                                (latitud, longitud),
                                fechaHoraUltimaActualizacion
                            )
                        );
                    }
                }
            }

            this.localizacionRecibida = null;
        }
    }
}
