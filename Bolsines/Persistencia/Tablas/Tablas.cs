namespace Bolsines.Persistencia.Tablas
{
    // ---------------------------------------------------------------------
    // Clases de persistencia (una por tabla de la base de datos).
    //
    // NO son las entidades del dominio: son "registros" planos con
    // propiedades públicas y claves (Id / FK) que Entity Framework Core
    // sabe mapear directamente a tablas.
    //
    // Las entidades del dominio (carpeta Entidades) quedan intactas, tal
    // como están en el diagrama de clases. El RepositorioBolsines se encarga
    // de traducir estos registros a objetos del dominio (patrón Data Mapper).
    // ---------------------------------------------------------------------

    public class EstadoBD
    {
        public int Id { get; set; }
        public string Ambito { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public string Nombre { get; set; } = "";
    }

    public class ComisionMedicaBD
    {
        public string Codigo { get; set; } = ""; // PK natural
        public string Direccion { get; set; } = "";
        public string Email { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Telefono { get; set; } = "";
    }

    public class BolsinBD
    {
        public int NumeroBolsin { get; set; } // PK natural
        public DateTime Fecha { get; set; }
        public int NumeroPrecinto { get; set; }
        public double Peso { get; set; }
        public string CodigoCMOrigen { get; set; } = ""; // FK -> ComisionMedica
        public string CodigoCMDestino { get; set; } = ""; // FK -> ComisionMedica
    }

    public class CambioEstadoBolsinBD
    {
        public int Id { get; set; }
        public int NumeroBolsin { get; set; } // FK -> Bolsin
        public int IdEstado { get; set; } // FK -> Estado
        public DateTime FechaHoraInicio { get; set; }
        public DateTime? FechaHoraFin { get; set; }
    }

    public class UsuarioBD
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public DateTime FechaAlta { get; set; }
        public string Contrasena { get; set; } = "";
    }

    public class RolBD
    {
        public int Id { get; set; }
        public string Descripcion { get; set; } = "";
        public string Nombre { get; set; } = "";
    }

    public class EmpleadoBD
    {
        public int Id { get; set; }
        public string Apellido { get; set; } = "";
        public string Email { get; set; } = "";
        public string Nombre { get; set; } = "";
        public int IdUsuario { get; set; } // FK -> Usuario
        public int IdRol { get; set; } // FK -> Rol
        public string CodigoCMAsignada { get; set; } = ""; // FK -> ComisionMedica
    }

    public class SesionBD
    {
        public int Id { get; set; }
        public DateTime FechaHoraInicio { get; set; }
        public DateTime? FechaHoraFin { get; set; }
        public int IdUsuario { get; set; } // FK -> Usuario
    }
}
