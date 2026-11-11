using Bolsines.Entidades;

namespace Bolsines.Interfaces.Patron_Adaptador
{
    public interface IAdaptadorGPSTracker
    {
        public List<(
            Bolsin bolsin,
            int numeroBolsin,
            (double latitud, double longitud) ubicacion,
            DateTime fechaHoraUbicacion
        )> recibirUbicacionBolsin();

        public void solicitarUbicacionBolsin();

        public void adaptarUbicacionBolsin();
    }
}

