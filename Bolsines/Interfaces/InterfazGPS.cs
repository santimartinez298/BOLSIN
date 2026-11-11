using Bolsines.Datos_Prueba;
using Bolsines.Entidades;
using Bolsines.Interfaces.Patron_Adaptador;

namespace Bolsines.Interfaces
{
    public class InterfazGPS
    {
        // Atributos
        private List<string> apiKeyGPSTrackers;
        private List<(
            Bolsin bolsin,
            int numeroBolsin,
            (double latitud, double longitud) ubicacion,
            DateTime fechaHoraUbicacion
        )> localizacionBolsinesAdaptada;
        private List<int> numeroBolsin;
        private string codigoCMOrigen;
        private string codigosCMDestino;

        // Constructor

        // public InterfazGPS(IAdaptadorGPSTracker adaptadorGPS)
        // {
        //     this.apiKeyGPSTrackers = DatosPrueba.getAPIKeyGPST
        //     this.adaptadorGPS = adaptadorGPS;
        // }

        public InterfazGPS()
        {
            this.apiKeyGPSTrackers = DatosPrueba.getAPIKeyGPSTrackers();
        }

        // Métodos
        public List<(
            Bolsin bolsin,
            int numeroBolsin,
            (double latitud, double longitud) ubicacion,
            DateTime fechaHoraUbicacion
        )> getBolsinLocation(
            List<(
                Bolsin bolsin,
                int numero
            )> bolsinesEnviados,
            string codigoCMOrigen
        )
        {
            IAdaptadorGPSTracker adaptadorGPSTracker = new AdaptadorGPSTrackerXTR4500L(
                this.apiKeyGPSTrackers[0],
                bolsinesEnviados,
                codigoCMOrigen
            );
            this.localizacionBolsinesAdaptada = adaptadorGPSTracker.recibirUbicacionBolsin();

            return this.localizacionBolsinesAdaptada;
        }
    }
}
