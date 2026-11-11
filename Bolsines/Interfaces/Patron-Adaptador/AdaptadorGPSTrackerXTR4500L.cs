using System.Text.Json;
using Bolsines.Datos_Prueba;
using Bolsines.Entidades;
using Bolsines.Interfaces.Dispositivos;

namespace Bolsines.Interfaces.Patron_Adaptador
{
    public class AdaptadorGPSTrackerXTR4500L : IAdaptadorGPSTracker
    {
        // Atributos
        private string apiKey;
        private List<(Bolsin bolsin, int numero)> bolsinesEnviados;
        private string codigoCMOrigen;
        private string localizacionRecibida;
        private List<(
            Bolsin bolsin,
            int numeroBolsin,
            (double latitud, double longitud) ubicacion,
            DateTime fechaHoraUbicacion
        )> localizacionAdaptada = new();
        private List<ServicioGPSTrackerXTR4500L> gpssXTR4500L;

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
            this.gpssXTR4500L = DatosPrueba.cargarGPSsXTR4500L();
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

        public void solicitarUbicacionBolsin()
        {
            foreach (var bolsin in this.bolsinesEnviados)
            {
                var numeroBolsin = bolsin.numero;

                foreach (var gps in this.gpssXTR4500L)
                {
                    var localizacion = gps.getBolsinLocation(
                        this.apiKey,
                        numeroBolsin,
                        this.codigoCMOrigen
                    );

                    if (localizacion is not null)
                    {
                        this.localizacionRecibida = localizacion;
                        this.adaptarUbicacionBolsin();
                    }
                }
            }
        }

        public void adaptarUbicacionBolsin()
        {
            if (this.localizacionRecibida == null) return;

            // 1. Parsear el JSON
            using JsonDocument doc = JsonDocument.Parse(this.localizacionRecibida);
            JsonElement root = doc.RootElement;

            // 2. Extraer números
            int numeroBolsin = root.GetProperty("numeroBolsin").GetInt32();
            double latitud = root.GetProperty("latitud").GetDouble();
            double longitud = root.GetProperty("longitud").GetDouble();

            // 3. Extraer fecha UTC
            DateTimeOffset fechaHoraUTC = root.GetProperty("fechaHoraUltimaActualizacion")
                .GetDateTimeOffset();

            // 4. Convertir al horario de Argentina (UTC-3)
            DateTimeOffset fechaHoraArgentina = fechaHoraUTC.ToOffset(TimeSpan.FromHours(-3));

            // 5. Convertir a DateTime
            DateTime fechaHoraUltimaActualizacion = fechaHoraArgentina.DateTime;

            foreach (var bolsin in bolsinesEnviados)
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

            this.localizacionRecibida = null;
        }
    }
}
