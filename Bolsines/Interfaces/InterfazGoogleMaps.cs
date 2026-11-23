using System.Globalization;
using Bolsines.Datos_Prueba;
using Bolsines.Interfaces.Sistemas_Externos;

namespace Bolsines.Interfaces
{
    // Como usamos Leaflet, esta clase debería llamarse InterfazLeaflet, pero
    // la dejamos así para no cambiar el nombre de la clase en los diagramas
    public class InterfazGoogleMaps
    {
        // Atributos
        private string apiKeyGoogleMaps;
        private ServidorGoogleMaps servidorGoogleMaps;

        // Constructor
        public InterfazGoogleMaps()
        {
            this.apiKeyGoogleMaps = DatosPrueba.getAPIKeyGoogleMaps();
            this.servidorGoogleMaps = DatosPrueba.cargarServidorGoogleMaps();
        }

        // Métodos
        // Recibe la localización en el formato del sistema (String[][]):
        // [0] número de bolsín, [1] latitud, [2] longitud, [3] fecha y hora
        public void mostrarBolsinesEnMapa(string[][] localizacionBolsines)
        {
            var localizacionParaServidor =
                new List<(
                    int numeroBolsin,
                    (double latitud, double longitud) ubicacion,
                    DateTime fechaHoraUbicacion
                )>();

            foreach (string[] localizacion in localizacionBolsines)
            {
                localizacionParaServidor.Add(
                    (
                        int.Parse(localizacion[0], CultureInfo.InvariantCulture),
                        (
                            double.Parse(localizacion[1], CultureInfo.InvariantCulture),
                            double.Parse(localizacion[2], CultureInfo.InvariantCulture)
                        ),
                        DateTime.Parse(localizacion[3], CultureInfo.InvariantCulture)
                    )
                );
            }

            this.servidorGoogleMaps.mostrarBolsinesEnMapa(localizacionParaServidor);
        }
    }
}
