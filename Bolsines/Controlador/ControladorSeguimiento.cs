using System.Globalization;
using Bolsines.Entidades;
using Bolsines.Interfaces;
using Bolsines.Interfaces.Pantallas;
using Bolsines.Persistencia;
using Microsoft.AspNetCore.Components;

namespace Bolsines.Controlador
{
    public class ControladorSeguimiento
    {
        // Atributos de interacción
        private TaskCompletionSource<bool> senalBotonAceptar = new();
        private NavigationManager navigationManager;

        // Métodos de interacción
        public void ConfirmacionAceptada()
        {
            this.senalBotonAceptar?.TrySetResult(true);
        }

        // Atributos (diagrama de clases del patrón Adapter)
        private int[] numeroBolsin = new int[0];
        private string codigoCMOrigen = "";
        private string codigoCMDestino = "";
        private string[][] localizacionBolsines = new string[0][];
        private Sesion sesionActual;
        private Usuario usuarioLogueado = null!;
        private ComisionMedica cmUsuarioLogueado = null!;
        private Bolsin[] bolsinesEnviados = new Bolsin[0];
        private Bolsin bolsinSeleccionado = null!;
        private ComisionMedica cmDestinoBolsinSeleccionado = null!;
        private string emailGCMDestino = "";

        // Objetos con los que colabora el controlador
        private PantallaSeguimiento pantallaSeguimiento;
        private List<Empleado> empleados;
        private List<Bolsin> bolsines;
        private InterfazGPS interfazGPS = null!;
        private InterfazGoogleMaps interfazGoogleMaps = null!;
        private RepositorioBolsines repositorio;

        // Columnas de cada fila de localizacionBolsines (String[][])
        private const int columnaNumero = 0;
        private const int columnaLatitud = 1;
        private const int columnaLongitud = 2;
        private const int columnaFechaHora = 3;

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
            if (this.bolsinesEnviados.Length == 0)
            {
                this.localizacionBolsines = new string[0][];
                this.mostrarUbicacionEnMapa();
                pantallaSeguimiento.informarNoHayBolsinesEnviados();
                return;
            }

            this.buscarLocalizacionBolsines();
            this.verificarBolsinesSinUbicacion();
            this.mostrarOpcionesFiltroCMDestino();
            this.mostrarUbicacionEnMapa();
            pantallaSeguimiento.pedirSeleccionBolsin(this.localizacionBolsines);
        }

        public void buscarCMUsuarioLogueado()
        {
            this.usuarioLogueado = sesionActual.getUsuario();

            foreach (Empleado empleado in this.empleados)
            {
                if (empleado.esTuUsuario(this.usuarioLogueado))
                {
                    var cm = empleado.mostrarCM();
                    this.cmUsuarioLogueado = cm.cm;
                    this.codigoCMOrigen = cm.codigo;
                    pantallaSeguimiento.mostrarCMUsuarioLogueado(cm.nombre, cm.codigo);
                    return;
                }
            }
        }

        public void obtenerBolsinesEnviados()
        {
            List<Bolsin> enviados = new List<Bolsin>();
            List<int> numeros = new List<int>();

            foreach (Bolsin bolsin in this.bolsines)
            {
                if (bolsin.esTuCMOrigen(this.cmUsuarioLogueado) && bolsin.sosEnviado())
                {
                    enviados.Add(bolsin);
                    numeros.Add(bolsin.getNumeroBolsin());
                }
            }

            this.bolsinesEnviados = enviados.ToArray();
            this.numeroBolsin = numeros.ToArray();
        }

        // buscarLocalizacionBolsines(): void  (diagrama de secuencia del Adapter)
        public void buscarLocalizacionBolsines()
        {
            // new(): InterfazGPS
            this.interfazGPS = new InterfazGPS();

            // getBolsinLocation(numeroBolsin: int[], codigoCMOrigen: String): String[][]
            this.localizacionBolsines = this.interfazGPS.getBolsinLocation(
                this.numeroBolsin,
                this.codigoCMOrigen
            );
        }

        // A3: el GPS Tracker no pudo informar la ubicación de uno o más bolsines
        public void verificarBolsinesSinUbicacion()
        {
            List<int> bolsinesSinUbicacion = new List<int>();

            foreach (int numero in this.numeroBolsin)
            {
                if (this.buscarLocalizacion(numero) is null)
                {
                    bolsinesSinUbicacion.Add(numero);
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

            foreach (Bolsin bolsin in this.bolsinesEnviados)
            {
                ComisionMedica cmDestino = bolsin.obtenerCMDestino();
                string codigo = cmDestino.getCodigo();

                if (!opciones.Exists(opcion => opcion.codigo == codigo))
                {
                    opciones.Add((codigo, cmDestino.getNombre()));
                }
            }

            pantallaSeguimiento.mostrarOpcionesFiltroCMDestino(opciones);
        }

        // mostrarUbicacionEnMapa(): void  (diagrama de secuencia del Adapter)
        public void mostrarUbicacionEnMapa()
        {
            this.interfazGoogleMaps = new InterfazGoogleMaps();
            this.interfazGoogleMaps.mostrarBolsinesEnMapa(this.localizacionBolsines);
        }

        // Paso 6: filtrar por número de precinto o CM destino
        public void tomarFiltro(int? numeroPrecinto, string? codigoCMDestinoFiltro)
        {
            List<string[]> filtradas = new List<string[]>();

            foreach (string[] localizacion in this.localizacionBolsines)
            {
                Bolsin? bolsin = this.buscarBolsinEnviado(this.leerNumero(localizacion));

                if (bolsin is null)
                {
                    continue;
                }

                bool coincidePrecinto =
                    numeroPrecinto is null || bolsin.getNumeroPrecinto() == numeroPrecinto.Value;
                bool coincideCMDestino =
                    codigoCMDestinoFiltro is null
                    || bolsin.obtenerCMDestino().getCodigo() == codigoCMDestinoFiltro;

                if (coincidePrecinto && coincideCMDestino)
                {
                    filtradas.Add(localizacion);
                }
            }

            // A2: no se encuentra bolsín con el número de precinto ingresado
            if (numeroPrecinto is not null && filtradas.Count == 0)
            {
                pantallaSeguimiento.informarPrecintoNoEncontrado(numeroPrecinto.Value);
            }

            string[][] localizacionFiltrada = filtradas.ToArray();
            this.interfazGoogleMaps = new InterfazGoogleMaps();
            this.interfazGoogleMaps.mostrarBolsinesEnMapa(localizacionFiltrada);
            pantallaSeguimiento.pedirSeleccionBolsin(localizacionFiltrada);
        }

        public void tomarSeleccionBolsin(int numeroBolsinSeleccionado)
        {
            Bolsin? bolsin = this.buscarBolsinEnviado(numeroBolsinSeleccionado);
            string[]? localizacion = this.buscarLocalizacion(numeroBolsinSeleccionado);

            if (bolsin is null || localizacion is null)
            {
                return;
            }

            this.bolsinSeleccionado = bolsin;
            this.cmDestinoBolsinSeleccionado = bolsin.obtenerCMDestino();
            this.codigoCMDestino = this.cmDestinoBolsinSeleccionado.getCodigo();

            // Paso 8: consulta si se desea enviar el correo al GCM destino
            pantallaSeguimiento.solicitarConfirmacionEnvioMail(
                numeroBolsinSeleccionado,
                bolsin.getNumeroPrecinto(),
                this.cmDestinoBolsinSeleccionado.getNombre(),
                this.leerFechaHora(localizacion),
                this.leerLatitud(localizacion),
                this.leerLongitud(localizacion)
            );
        }

        // A5: el EB no selecciona la opción para enviar el correo electrónico
        public void tomarNoEnvioCorreo()
        {
            pantallaSeguimiento.informarCorreoNoEnviado(this.bolsinSeleccionado.getNumeroBolsin());
        }

        public async Task tomarConfirmacion()
        {
            this.obtenerCorreoGerente();
            this.llamarCU30();
            this.pantallaSeguimiento.informarEnvioMail(this.emailGCMDestino);
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
                if (empleado.esTuCM(this.cmDestinoBolsinSeleccionado) && empleado.sosGCM())
                {
                    this.emailGCMDestino = empleado.getEmail();
                    Console.WriteLine("Correo del gerente de CM destino: " + this.emailGCMDestino);
                    return;
                }
            }
        }

        public void llamarCU30()
        {
            int numero = this.bolsinSeleccionado.getNumeroBolsin();
            string[] localizacion = this.buscarLocalizacion(numero)!;

            CU_30_Notificar_Ubicacion_de_Bolsin cu30 = new CU_30_Notificar_Ubicacion_de_Bolsin();
            cu30.include(
                this.emailGCMDestino,
                numero,
                this.leerFechaHora(localizacion),
                this.leerLatitud(localizacion),
                this.leerLongitud(localizacion)
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

        // ----- Auxiliares para leer las filas de localizacionBolsines -----
        private string[]? buscarLocalizacion(int numero)
        {
            foreach (string[] localizacion in this.localizacionBolsines)
            {
                if (this.leerNumero(localizacion) == numero)
                {
                    return localizacion;
                }
            }
            return null;
        }

        private Bolsin? buscarBolsinEnviado(int numero)
        {
            foreach (Bolsin bolsin in this.bolsinesEnviados)
            {
                if (bolsin.getNumeroBolsin() == numero)
                {
                    return bolsin;
                }
            }
            return null;
        }

        private int leerNumero(string[] localizacion)
        {
            return int.Parse(localizacion[columnaNumero], CultureInfo.InvariantCulture);
        }

        private double leerLatitud(string[] localizacion)
        {
            return double.Parse(localizacion[columnaLatitud], CultureInfo.InvariantCulture);
        }

        private double leerLongitud(string[] localizacion)
        {
            return double.Parse(localizacion[columnaLongitud], CultureInfo.InvariantCulture);
        }

        private DateTime leerFechaHora(string[] localizacion)
        {
            return DateTime.Parse(localizacion[columnaFechaHora], CultureInfo.InvariantCulture);
        }
    }
}
