using Bolsines.Datos_Prueba;
using Bolsines.Interfaces.Patron_Adaptador;

namespace Bolsines.Interfaces
{
    // ---------------------------------------------------------------------
    // CLIENT del patrón Adapter: usa el GPS a través de IAdaptadorGPSTracker,
    // sin conocer el formato propio de cada modelo de dispositivo.
    // ---------------------------------------------------------------------
    public class InterfazGPS
    {
        // Atributos
        private string[] apiKeyGPSTrackers;
        private string[][] localizacionBolsinesAdaptada = new string[0][];
        private int[] numeroBolsin = new int[0];
        private string codigoCMOrigen = "";

        // Lo usaría el modelo NavTrack QX-7A, que busca por CM destino
        private string codigoCMDestino = "";

        // Constructor: new()
        public InterfazGPS()
        {
            this.apiKeyGPSTrackers = DatosPrueba.getAPIKeyGPSTrackers();
        }

        // Métodos
        // getBolsinLocation(numeroBolsin: int[], codigoCMOrigen: String): String[][]
        public string[][] getBolsinLocation(int[] numeroBolsin, string codigoCMOrigen)
        {
            this.numeroBolsin = numeroBolsin;
            this.codigoCMOrigen = codigoCMOrigen;

            // new(apiKey, numeroBolsin, codigoCMOrigen): AdaptadorGPSTrackerXTR4500L
            IAdaptadorGPSTracker adaptadorGPSTracker = new AdaptadorGPSTrackerXTR4500L(
                this.apiKeyGPSTrackers[0],
                this.numeroBolsin,
                this.codigoCMOrigen
            );

            // recibirUbicacionBolsin(): String[][]
            this.localizacionBolsinesAdaptada = adaptadorGPSTracker.recibirUbicacionBolsin();

            return this.localizacionBolsinesAdaptada;
        }
    }
}
