namespace Bolsines.Interfaces.Patron_Adaptador
{
    // ---------------------------------------------------------------------
    // TARGET del patrón Adapter: interfaz que el sistema espera de cualquier
    // GPS Tracker, sin importar el modelo.
    //
    // La ubicación adaptada se devuelve como String[][]: una fila por bolsín
    //   [0] número de bolsín
    //   [1] latitud
    //   [2] longitud
    //   [3] fecha y hora de la última actualización (yyyy-MM-ddTHH:mm:ss)
    // ---------------------------------------------------------------------
    public interface IAdaptadorGPSTracker
    {
        public string[][] recibirUbicacionBolsin();

        public void solicitarUbicacionBolsin();

        public void adaptarUbicacionBolsin();
    }
}
