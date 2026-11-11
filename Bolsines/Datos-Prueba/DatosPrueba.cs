using Bolsines.Entidades;
using Bolsines.Interfaces.Dispositivos;
using Bolsines.Interfaces.Sistemas_Externos;

namespace Bolsines.Datos_Prueba
{
    public class DatosPrueba
    {
        // Los objetos del dominio (Estado, Bolsin, Empleado, Sesion, etc.) ya NO
        // se crean acá: se persisten en la base de datos SQLite y se recuperan
        // con Persistencia/RepositorioBolsines.cs (datos iniciales en
        // Persistencia/DatosIniciales.cs).
        //
        // Este archivo sólo conserva la SIMULACIÓN de sistemas externos
        // (dispositivos GPS, API keys y servidor de mapas).

        // API Keys de los GPS Trackers (las de la API de la cátedra, dsi-api-bolsines).
        // InterfazGPS usa la primera (XTR-4500L). Una API Key de otro modelo -> HTTP 401.
        public static List<string> getAPIKeyGPSTrackers()
        {
            List<string> apiKeyGPSTrackers = new List<string>();
            apiKeyGPSTrackers.Add("sk_xtr_9F3a7C21bE6d4A10f2c8e5b7d913"); // XTR-4500L
            apiKeyGPSTrackers.Add("sk_nav_7b61d9e4c3a2485fae10c77d5b2f"); // NavTrack QX-7A
            apiKeyGPSTrackers.Add("sk_geo_f2d84a1c9e7345b8a6d1c3f0972e"); // GeoPulse MTR-900
            return apiKeyGPSTrackers;
        }

        // API Key de Google Maps
        public static string getAPIKeyGoogleMaps()
        {
            return "AIzaSyDQV7y8a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0u1v2w3x4y5z6";
        }

        // Objeto :ServidorGoogleMaps
        public static readonly ServidorGoogleMaps servidorGoogleMaps =
            new ServidorGoogleMaps();

        public static ServidorGoogleMaps cargarServidorGoogleMaps()
        {
            return servidorGoogleMaps;
        }
    }
}
