using Bolsines.Entidades;
using Bolsines.Persistencia.Tablas;
using Microsoft.EntityFrameworkCore;

namespace Bolsines.Persistencia
{
    // ---------------------------------------------------------------------
    // Repositorio: único punto de acceso a la base de datos para el resto
    // del sistema (controlador y pantallas).
    //
    // - Lee las tablas con Entity Framework Core (ContextoBolsines).
    // - Traduce cada registro a su objeto del dominio usando los
    //   constructores de las entidades (patrón Data Mapper).
    // - Mantiene un "mapa de identidad" (Identity Map): cada fila se
    //   convierte en UN SOLO objeto. Por ejemplo, el Usuario de la sesión
    //   y el Usuario del empleado son la misma instancia, y por eso siguen
    //   funcionando las comparaciones por referencia (==) que usan
    //   esTuUsuario(), esTuCM() y esTuCMOrigen().
    //
    // Por eso, dentro de un mismo caso de uso hay que usar UNA sola
    // instancia del repositorio.
    // ---------------------------------------------------------------------
    public class RepositorioBolsines
    {
        // Mapas de identidad (clave de la tabla -> objeto del dominio)
        private readonly Dictionary<int, Estado> estados = new();
        private readonly Dictionary<string, ComisionMedica> comisionesMedicas = new();
        private readonly Dictionary<int, Usuario> usuarios = new();
        private readonly Dictionary<int, Rol> roles = new();
        private bool catalogosCargados = false;

        // Devuelve la sesión abierta (la que no tiene fecha/hora de fin).
        public Sesion cargarSesionActual()
        {
            using var contexto = new ContextoBolsines();
            this.cargarCatalogos(contexto);

            SesionBD? sesionBD = contexto
                .Sesiones.AsNoTracking()
                .Where(s => s.FechaHoraFin == null)
                .OrderByDescending(s => s.FechaHoraInicio)
                .FirstOrDefault();

            if (sesionBD is null)
            {
                throw new InvalidOperationException("No hay ninguna sesión activa en la base de datos.");
            }

            return new Sesion(
                sesionBD.FechaHoraInicio,
                sesionBD.FechaHoraFin,
                this.usuarios[sesionBD.IdUsuario]
            );
        }

        public List<Empleado> cargarEmpleados()
        {
            using var contexto = new ContextoBolsines();
            this.cargarCatalogos(contexto);

            List<Empleado> empleados = new List<Empleado>();

            foreach (EmpleadoBD e in contexto.Empleados.AsNoTracking().OrderBy(fila => fila.Id))
            {
                empleados.Add(
                    new Empleado(
                        e.Apellido,
                        e.Email,
                        e.Nombre,
                        this.usuarios[e.IdUsuario],
                        this.roles[e.IdRol],
                        this.comisionesMedicas[e.CodigoCMAsignada]
                    )
                );
            }

            return empleados;
        }

        public List<Bolsin> cargarBolsines()
        {
            using var contexto = new ContextoBolsines();
            this.cargarCatalogos(contexto);

            // Se traen todos los cambios de estado en una sola consulta y se
            // agrupan por número de bolsín.
            Dictionary<int, List<CambioEstadoBolsinBD>> cambiosPorBolsin = contexto
                .CambiosEstadoBolsin.AsNoTracking()
                .ToList()
                .GroupBy(ce => ce.NumeroBolsin)
                .ToDictionary(grupo => grupo.Key, grupo => grupo.OrderBy(ce => ce.FechaHoraInicio).ToList());

            List<Bolsin> bolsines = new List<Bolsin>();

            foreach (BolsinBD b in contexto.Bolsines.AsNoTracking().OrderBy(fila => fila.NumeroBolsin))
            {
                List<CambioEstadoBolsin> cambiosEstado = new List<CambioEstadoBolsin>();

                if (cambiosPorBolsin.TryGetValue(b.NumeroBolsin, out var cambiosBD))
                {
                    foreach (CambioEstadoBolsinBD ce in cambiosBD)
                    {
                        cambiosEstado.Add(
                            new CambioEstadoBolsin(
                                ce.FechaHoraInicio,
                                ce.FechaHoraFin,
                                this.estados[ce.IdEstado]
                            )
                        );
                    }
                }

                bolsines.Add(
                    new Bolsin(
                        b.Fecha,
                        b.NumeroBolsin,
                        b.NumeroPrecinto,
                        b.Peso,
                        this.comisionesMedicas[b.CodigoCMOrigen],
                        this.comisionesMedicas[b.CodigoCMDestino],
                        cambiosEstado
                    )
                );
            }

            return bolsines;
        }

        // Carga (una sola vez por repositorio) las tablas que son referenciadas
        // por otras: Estado, ComisionMedica, Usuario y Rol.
        private void cargarCatalogos(ContextoBolsines contexto)
        {
            if (this.catalogosCargados)
            {
                return;
            }

            foreach (EstadoBD e in contexto.Estados.AsNoTracking())
            {
                this.estados[e.Id] = new Estado(e.Ambito, e.Descripcion, e.Nombre);
            }

            foreach (ComisionMedicaBD cm in contexto.ComisionesMedicas.AsNoTracking())
            {
                this.comisionesMedicas[cm.Codigo] = new ComisionMedica(
                    cm.Codigo,
                    cm.Direccion,
                    cm.Email,
                    cm.Nombre,
                    cm.Telefono
                );
            }

            foreach (UsuarioBD u in contexto.Usuarios.AsNoTracking())
            {
                this.usuarios[u.Id] = new Usuario(u.Nombre, u.FechaAlta, u.Contrasena);
            }

            foreach (RolBD r in contexto.Roles.AsNoTracking())
            {
                this.roles[r.Id] = new Rol(r.Descripcion, r.Nombre);
            }

            this.catalogosCargados = true;
        }
    }
}
