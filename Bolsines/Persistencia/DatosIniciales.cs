using Bolsines.Persistencia.Tablas;
using Microsoft.EntityFrameworkCore;

namespace Bolsines.Persistencia
{
    // ---------------------------------------------------------------------
    // Datos iniciales (seed) de la base de datos.
    //
    // Se insertan como filas la primera vez que se crea la base
    // (Database.EnsureCreated en Program.cs).
    //
    // Los números de bolsín y los códigos de comisión médica coinciden con
    // los que conoce la API del GPS de la cátedra (dsi-api-bolsines,
    // InMemoryBolsinRepository): bolsines 101..110, códigos "CM-XXX-NN".
    // Si no coinciden, la API responde 404 y el bolsín no aparece en el mapa.
    //
    // IMPORTANTE: si se modifica algo de este archivo, hay que borrar el
    // archivo bolsines.db para que la base se vuelva a generar.
    // ---------------------------------------------------------------------
    public static class DatosIniciales
    {
        public static void Cargar(ModelBuilder modelBuilder)
        {
            // ----- Estados -----
            modelBuilder.Entity<EstadoBD>().HasData(
                new EstadoBD { Id = 1, Ambito = "BOLSIN", Descripcion = "Descripcion1", Nombre = "CREADO" },
                new EstadoBD { Id = 2, Ambito = "BOLSIN", Descripcion = "Descripcion2", Nombre = "CERRADO" },
                new EstadoBD { Id = 3, Ambito = "BOLSIN", Descripcion = "Descripcion3", Nombre = "ENVIADO" },
                new EstadoBD { Id = 4, Ambito = "BOLSIN", Descripcion = "Descripcion4", Nombre = "RECIBIDO_EN_CM_DESTINO" },
                new EstadoBD { Id = 5, Ambito = "BOLSIN", Descripcion = "Descripcion5", Nombre = "DE_BAJA" }
            );

            // ----- Comisiones Médicas (códigos iguales a los de la API) -----
            modelBuilder.Entity<ComisionMedicaBD>().HasData(
                new ComisionMedicaBD
                {
                    Codigo = "CM-CBA-01",
                    Direccion = "Av. Colón 1150",
                    Email = "cordoba@srt.gob.ar",
                    Nombre = "Comisión Médica Córdoba",
                    Telefono = "0800-666-6778",
                },
                new ComisionMedicaBD
                {
                    Codigo = "CM-ROS-02",
                    Direccion = "Córdoba 1452",
                    Email = "rosario@srt.gob.ar",
                    Nombre = "Comisión Médica Rosario",
                    Telefono = "0800-666-6779",
                },
                new ComisionMedicaBD
                {
                    Codigo = "CM-MDZ-03",
                    Direccion = "Av. San Martín 1020",
                    Email = "mendoza@srt.gob.ar",
                    Nombre = "Comisión Médica Mendoza",
                    Telefono = "0800-666-6780",
                },
                new ComisionMedicaBD
                {
                    Codigo = "CM-BA-01",
                    Direccion = "Moreno 401",
                    Email = "sedecentral@srt.gob.ar",
                    Nombre = "Comisión Médica CABA - Sede Central",
                    Telefono = "0800-666-6781",
                }
            );

            // ----- Bolsines -----
            // 101..104: existen en la API con el mismo origen y destino.
            // 111: NO existe en la API (el GPS responde 404) -> no aparece en el mapa.
            modelBuilder.Entity<BolsinBD>().HasData(
                new BolsinBD { NumeroBolsin = 101, Fecha = new DateTime(2026, 2, 12), NumeroPrecinto = 1437, Peso = 427.35, CodigoCMOrigen = "CM-CBA-01", CodigoCMDestino = "CM-ROS-02" },
                new BolsinBD { NumeroBolsin = 102, Fecha = new DateTime(2026, 3, 1), NumeroPrecinto = 5284, Peso = 918.72, CodigoCMOrigen = "CM-ROS-02", CodigoCMDestino = "CM-MDZ-03" },
                new BolsinBD { NumeroBolsin = 103, Fecha = new DateTime(2026, 2, 27), NumeroPrecinto = 8916, Peso = 1364.18, CodigoCMOrigen = "CM-MDZ-03", CodigoCMDestino = "CM-BA-01" },
                new BolsinBD { NumeroBolsin = 104, Fecha = new DateTime(2026, 2, 20), NumeroPrecinto = 3172, Peso = 742.60, CodigoCMOrigen = "CM-BA-01", CodigoCMDestino = "CM-CBA-01" },
                new BolsinBD { NumeroBolsin = 111, Fecha = new DateTime(2026, 2, 25), NumeroPrecinto = 6630, Peso = 512.40, CodigoCMOrigen = "CM-CBA-01", CodigoCMDestino = "CM-MDZ-03" }
            );

            // ----- Cambios de estado de cada bolsín -----
            // FechaHoraFin = null  =>  es el cambio de estado ACTUAL
            modelBuilder.Entity<CambioEstadoBolsinBD>().HasData(
                // Bolsín 101: CREADO -> CERRADO -> ENVIADO (actual)
                new CambioEstadoBolsinBD { Id = 1, NumeroBolsin = 101, IdEstado = 1, FechaHoraInicio = new DateTime(2026, 2, 12, 9, 15, 23), FechaHoraFin = new DateTime(2026, 3, 2, 14, 30, 41) },
                new CambioEstadoBolsinBD { Id = 2, NumeroBolsin = 101, IdEstado = 2, FechaHoraInicio = new DateTime(2026, 3, 2, 14, 30, 41), FechaHoraFin = new DateTime(2026, 3, 7, 8, 45, 8) },
                new CambioEstadoBolsinBD { Id = 3, NumeroBolsin = 101, IdEstado = 3, FechaHoraInicio = new DateTime(2026, 3, 7, 8, 45, 8), FechaHoraFin = null },

                // Bolsín 102: CREADO (actual)
                new CambioEstadoBolsinBD { Id = 4, NumeroBolsin = 102, IdEstado = 1, FechaHoraInicio = new DateTime(2026, 3, 1, 8, 20, 57), FechaHoraFin = null },

                // Bolsín 103: CREADO -> CERRADO (actual)
                new CambioEstadoBolsinBD { Id = 5, NumeroBolsin = 103, IdEstado = 1, FechaHoraInicio = new DateTime(2026, 2, 27, 10, 5, 12), FechaHoraFin = new DateTime(2026, 3, 6, 16, 10, 49) },
                new CambioEstadoBolsinBD { Id = 6, NumeroBolsin = 103, IdEstado = 2, FechaHoraInicio = new DateTime(2026, 3, 6, 16, 10, 49), FechaHoraFin = null },

                // Bolsín 104: CREADO -> CERRADO -> ENVIADO (actual)
                new CambioEstadoBolsinBD { Id = 7, NumeroBolsin = 104, IdEstado = 1, FechaHoraInicio = new DateTime(2026, 2, 20, 9, 14, 23), FechaHoraFin = new DateTime(2026, 3, 4, 16, 42, 17) },
                new CambioEstadoBolsinBD { Id = 8, NumeroBolsin = 104, IdEstado = 2, FechaHoraInicio = new DateTime(2026, 3, 4, 16, 42, 17), FechaHoraFin = new DateTime(2026, 3, 7, 10, 15, 34) },
                new CambioEstadoBolsinBD { Id = 9, NumeroBolsin = 104, IdEstado = 3, FechaHoraInicio = new DateTime(2026, 3, 7, 10, 15, 34), FechaHoraFin = null },

                // Bolsín 111: CREADO -> CERRADO -> ENVIADO (actual), sin GPS en la API
                new CambioEstadoBolsinBD { Id = 10, NumeroBolsin = 111, IdEstado = 1, FechaHoraInicio = new DateTime(2026, 2, 25, 11, 2, 40), FechaHoraFin = new DateTime(2026, 3, 5, 9, 30, 0) },
                new CambioEstadoBolsinBD { Id = 11, NumeroBolsin = 111, IdEstado = 2, FechaHoraInicio = new DateTime(2026, 3, 5, 9, 30, 0), FechaHoraFin = new DateTime(2026, 3, 7, 11, 0, 0) },
                new CambioEstadoBolsinBD { Id = 12, NumeroBolsin = 111, IdEstado = 3, FechaHoraInicio = new DateTime(2026, 3, 7, 11, 0, 0), FechaHoraFin = null }
            );

            // ----- Usuarios -----
            modelBuilder.Entity<UsuarioBD>().HasData(
                new UsuarioBD { Id = 1, Nombre = "marcela.cattaneo", FechaAlta = new DateTime(2022, 3, 14), Contrasena = "Bolsin404NoEncontrado" },
                new UsuarioBD { Id = 2, Nombre = "german.velez", FechaAlta = new DateTime(2023, 6, 5), Contrasena = "Comision66SinPapeles" },
                new UsuarioBD { Id = 3, Nombre = "salvador.barbera", FechaAlta = new DateTime(2023, 10, 21), Contrasena = "PDF999Paginas" },
                new UsuarioBD { Id = 4, Nombre = "federico.mizzau", FechaAlta = new DateTime(2024, 2, 15), Contrasena = "Comision24x7" },
                new UsuarioBD { Id = 5, Nombre = "joaquin.bonugli", FechaAlta = new DateTime(2024, 8, 14), Contrasena = "FolioVaFolioViene" }
            );

            // ----- Roles -----
            modelBuilder.Entity<RolBD>().HasData(
                new RolBD { Id = 1, Descripcion = "Descripcion1", Nombre = "ENCARGADO_DE_BOLSINES" },
                new RolBD { Id = 2, Descripcion = "Descripcion2", Nombre = "ENCARGADO_DE_DOCUMENTACION" },
                new RolBD { Id = 3, Descripcion = "Descripcion3", Nombre = "GERENTE_DE_COMISION_MEDICA" },
                new RolBD { Id = 4, Descripcion = "Descripcion4", Nombre = "PARAMETRIZADOR_DEL_SISTEMA" }
            );

            // ----- Empleados -----
            // Marcela (usuario logueado) trabaja en CM-CBA-01 -> ve los bolsines enviados desde Córdoba.
            // Salvador es GCM de Rosario (destino del 101); Joaquín es GCM de Mendoza (destino del 111).
            modelBuilder.Entity<EmpleadoBD>().HasData(
                new EmpleadoBD { Id = 1, Apellido = "Cattaneo", Email = "mfcattaneo@gmail.com", Nombre = "Marcela", IdUsuario = 1, IdRol = 1, CodigoCMAsignada = "CM-CBA-01" },
                new EmpleadoBD { Id = 2, Apellido = "Vélez", Email = "germanevelez@hotmail.com", Nombre = "Germán", IdUsuario = 2, IdRol = 2, CodigoCMAsignada = "CM-CBA-01" },
                new EmpleadoBD { Id = 3, Apellido = "Barbera", Email = "jsbarbera4@gmail.com", Nombre = "Salvador", IdUsuario = 3, IdRol = 3, CodigoCMAsignada = "CM-ROS-02" },
                new EmpleadoBD { Id = 4, Apellido = "Mizzau", Email = "federicomizzau.77@gmail.com", Nombre = "Federico", IdUsuario = 4, IdRol = 4, CodigoCMAsignada = "CM-MDZ-03" },
                new EmpleadoBD { Id = 5, Apellido = "Bonugli", Email = "joaquin.bonugli@gmail.com", Nombre = "Joaquín", IdUsuario = 5, IdRol = 3, CodigoCMAsignada = "CM-MDZ-03" }
            );

            // ----- Sesiones -----
            // La sesión ACTUAL es la que no tiene fecha de fin.
            modelBuilder.Entity<SesionBD>().HasData(
                new SesionBD { Id = 1, FechaHoraInicio = new DateTime(2026, 11, 9, 8, 30, 0), FechaHoraFin = new DateTime(2026, 11, 9, 17, 0, 0), IdUsuario = 2 },
                new SesionBD { Id = 2, FechaHoraInicio = new DateTime(2026, 11, 10, 9, 0, 0), FechaHoraFin = null, IdUsuario = 1 }
            );
        }
    }
}
