using Bolsines.Entidades;
using Bolsines.Interfaces;
// using Bolsines.Interfaces.Patron_Adaptador;
using Bolsines.Interfaces.Pantallas;
using Bolsines.Persistencia;
using Microsoft.AspNetCore.Components;

namespace Bolsines.Controlador
{
    public class ControladorSeguimiento
    {
        // Atributos de interacción
        private TaskCompletionSource<bool> senalBotonAceptar;
        private NavigationManager navigationManager;

        // Métodos de interacción
        public void ConfirmacionAceptada()
        {
            this.senalBotonAceptar?.TrySetResult(true);
        }

        // Atributos
        private Sesion sesionActual;
        private Usuario usuarioLogueado;
        private (ComisionMedica cm, string nombre, string codigo) cmUsuarioLogueado;
        private List<(Bolsin bolsin, int numero)> bolsinesEnviados = new();
        private List<(
            Bolsin bolsin,
            int numeroBolsin,
            (double latitud, double longitud) ubicacion,
            DateTime fechaHoraUbicacion
        )> localizacionBolsines;
        private (
            Bolsin bolsin,
            int numero,
            (double latitud, double longitud) ubicacion,
            DateTime fechaHoraUbicacion
        ) bolsinSeleccionado;
        private string emailGCMDestino;
        private PantallaSeguimiento pantallaSeguimiento;
        private List<Empleado> empleados;
        private List<Bolsin> bolsines;
        private InterfazGPS interfazGPS;
        private InterfazGoogleMaps interfazGoogleMaps;
        private RepositorioBolsines repositorio;

        // Constructor
        public ControladorSeguimiento(
            PantallaSeguimiento pantallaSeguimiento,
            NavigationManager navigationManager
        )
        {
            // Persistencia: los objetos se recuperan de la base de datos.
            // Se usa UNA sola instancia del repositorio para que los objetos
            // compartidos (Usuario, ComisionMedica, etc.) sean las mismas
            // instancias y funcionen las comparaciones de las entidades.
            this.repositorio = new RepositorioBolsines();
            this.sesionActual = this.repositorio.cargarSesionActual();
            this.pantallaSeguimiento = pantallaSeguimiento;
            this.empleados = this.repositorio.cargarEmpleados();
            this.bolsines = this.repositorio.cargarBolsines();
            this.navigationManager = navigationManager;
        }

        // Métodos
        public void nuevoSeguimientoBolsines()
        {
            this.buscarCMUsuarioLogueado();
            this.obtenerBolsinesEnviados();

            // A1: no se encuentran bolsines en estado enviado
            if (this.bolsinesEnviados.Count == 0)
            {
                this.localizacionBolsines = new();
                this.mostrarUbicacionEnMapa(this.localizacionBolsines);
                pantallaSeguimiento.informarNoHayBolsinesEnviados();
                return;
            }

            this.buscarLocalizacionBolsines();
            this.verificarBolsinesSinUbicacion();
            this.mostrarOpcionesFiltroCMDestino();
            this.mostrarUbicacionEnMapa(this.localizacionBolsines);
            pantallaSeguimiento.pedirSeleccionBolsin(this.localizacionBolsines);
        }

        public void buscarCMUsuarioLogueado()
        {
            this.usuarioLogueado = sesionActual.getUsuario();

            foreach (Empleado empleado in this.empleados)
            {
                if (empleado.esTuUsuario(this.usuarioLogueado))
                {
                    this.cmUsuarioLogueado = empleado.mostrarCM();
                    pantallaSeguimiento.mostrarCMUsuarioLogueado(
                        this.cmUsuarioLogueado.nombre,
                        this.cmUsuarioLogueado.codigo
                    );
                    return;
                }
            }
        }

        public void obtenerBolsinesEnviados()
        {
            foreach (Bolsin bolsin in this.bolsines)
            {
                if (bolsin.esTuCMOrigen(this.cmUsuarioLogueado.cm) && bolsin.sosEnviado())
                {
                    var numeroBolsin = bolsin.getNumeroBolsin();
                    this.bolsinesEnviados.Add((bolsin, numeroBolsin));
                }
            }
        }

        public void buscarLocalizacionBolsines()
        {
            // Habría que pasarle datos que no están definidos como atributos en el controlador.
            // Preguntar si está bien aplicado el patrón Adapter de la manera que ya había hecho.
            //
            // IAdaptadorGPSTracker adaptadorGPS = new AdaptadorGPSTrackerXTR4500L(...);
            //
            // this.interfazGPS = new InterfazGPS(adaptadorGPS);

            this.interfazGPS = new InterfazGPS();
            this.localizacionBolsines = this.interfazGPS.getBolsinLocation(
                this.bolsinesEnviados,
                this.cmUsuarioLogueado.codigo
            );
        }

        // A3: el GPS Tracker no pudo informar la ubicación de uno o más bolsines
        public void verificarBolsinesSinUbicacion()
        {
            List<int> bolsinesSinUbicacion = new List<int>();

            foreach (var enviado in this.bolsinesEnviados)
            {
                bool tieneUbicacion = false;

                foreach (var localizacion in this.localizacionBolsines)
                {
                    if (localizacion.numeroBolsin == enviado.numero)
                    {
                        tieneUbicacion = true;
                        break;
                    }
                }

                if (!tieneUbicacion)
                {
                    bolsinesSinUbicacion.Add(enviado.numero);
                }
            }

            if (bolsinesSinUbicacion.Count > 0)
            {
                pantallaSeguimiento.informarBolsinesSinUbicacion(bolsinesSinUbicacion);
            }
        }

        // Paso 6: opciones para filtrar por CM destino
        public void mostrarOpcionesFiltroCMDestino()
        {
            List<(string codigo, string nombre)> opciones = new List<(string codigo, string nombre)>();

            foreach (var enviado in this.bolsinesEnviados)
            {
                ComisionMedica cmDestino = enviado.bolsin.obtenerCMDestino();
                string codigo = cmDestino.getCodigo();

                if (!opciones.Exists(opcion => opcion.codigo == codigo))
                {
                    opciones.Add((codigo, cmDestino.getNombre()));
                }
            }

            pantallaSeguimiento.mostrarOpcionesFiltroCMDestino(opciones);
        }

        public void mostrarUbicacionEnMapa(
            List<(
                Bolsin bolsin,
                int numeroBolsin,
                (double latitud, double longitud) ubicacion,
                DateTime fechaHoraUbicacion
            )> localizacionesAMostrar
        )
        {
            this.interfazGoogleMaps = new InterfazGoogleMaps();
            this.interfazGoogleMaps.mostrarBolsinesEnMapa(localizacionesAMostrar);
        }

        // Paso 6: filtrar por número de precinto o CM destino
        public void tomarFiltro(int? numeroPrecinto, string? codigoCMDestino)
        {
            var bolsinesFiltrados = new List<(
                Bolsin bolsin,
                int numeroBolsin,
                (double latitud, double longitud) ubicacion,
                DateTime fechaHoraUbicacion
            )>();

            foreach (var localizacion in this.localizacionBolsines)
            {
                bool coincidePrecinto =
                    numeroPrecinto is null
                    || localizacion.bolsin.getNumeroPrecinto() == numeroPrecinto.Value;
                bool coincideCMDestino =
                    codigoCMDestino is null
                    || localizacion.bolsin.obtenerCMDestino().getCodigo() == codigoCMDestino;

                if (coincidePrecinto && coincideCMDestino)
                {
                    bolsinesFiltrados.Add(localizacion);
                }
            }

            // A2: no se encuentra bolsín con el número de precinto ingresado
            if (numeroPrecinto is not null && bolsinesFiltrados.Count == 0)
            {
                pantallaSeguimiento.informarPrecintoNoEncontrado(numeroPrecinto.Value);
            }

            this.mostrarUbicacionEnMapa(bolsinesFiltrados);
            pantallaSeguimiento.pedirSeleccionBolsin(bolsinesFiltrados);
        }

        public void tomarSeleccionBolsin(
            (
                Bolsin bolsin,
                int numero,
                (double latitud, double longitud) ubicacion,
                DateTime fechaHoraUbicacion
            ) bolsinSeleccionado
        )
        {
            this.bolsinSeleccionado = bolsinSeleccionado;
            ComisionMedica cmDestino = bolsinSeleccionado.bolsin.obtenerCMDestino();

            // Paso 8: consulta si se desea enviar el correo al GCM destino
            pantallaSeguimiento.solicitarConfirmacionEnvioMail(
                bolsinSeleccionado.numero,
                bolsinSeleccionado.bolsin.getNumeroPrecinto(),
                cmDestino.getNombre(),
                bolsinSeleccionado.fechaHoraUbicacion,
                bolsinSeleccionado.ubicacion.latitud,
                bolsinSeleccionado.ubicacion.longitud
            );
        }

        // A5: el EB no selecciona la opción para enviar el correo electrónico
        public void tomarNoEnvioCorreo()
        {
            pantallaSeguimiento.informarCorreoNoEnviado(this.bolsinSeleccionado.numero);
        }

        public async Task tomarConfirmacion()
        {
            this.obtenerCorreoGerente();
            this.llamarCU30();
            this.pantallaSeguimiento.informarEnvioMail(this.emailGCMDestino ?? "");
            this.senalBotonAceptar = new TaskCompletionSource<bool>();

            Task usuarioAcepto = senalBotonAceptar.Task;
            Task finalizoTemporizador = Task.Delay(60000);

            await Task.WhenAny(usuarioAcepto, finalizoTemporizador);
            this.finCU();
        }

        public void obtenerCorreoGerente()
        {
            foreach (Empleado empleado in this.empleados)
            {
                if (
                    empleado.esTuCM(this.bolsinSeleccionado.bolsin.obtenerCMDestino())
                    && empleado.sosGCM()
                )
                {
                    this.emailGCMDestino = empleado.getEmail();
                    Console.WriteLine("Correo del gerente de CM destino: " + this.emailGCMDestino);
                    return;
                }
            }
        }

        public void llamarCU30()
        {
            CU_30_Notificar_Ubicacion_de_Bolsin cu30 = new CU_30_Notificar_Ubicacion_de_Bolsin();
            cu30.include(
                this.emailGCMDestino,
                this.bolsinSeleccionado.numero,
                this.bolsinSeleccionado.fechaHoraUbicacion,
                this.bolsinSeleccionado.ubicacion.latitud,
                this.bolsinSeleccionado.ubicacion.longitud
            );
            Console.WriteLine("CU 30 llamado con éxito");
        }

        // A4: el EB cancela la ejecución del caso de uso
        public void cancelarCU()
        {
            this.finCU();
        }

        public void finCU()
        {
            this.navigationManager.NavigateTo("/");
        }
    }
}
