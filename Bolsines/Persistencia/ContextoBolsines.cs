using Bolsines.Persistencia.Tablas;
using Microsoft.EntityFrameworkCore;

namespace Bolsines.Persistencia
{
    // ---------------------------------------------------------------------
    // Contexto de Entity Framework Core.
    //
    // Representa la sesión con la base de datos SQLite: define qué tablas
    // existen (DbSet), cómo se llaman, sus claves, relaciones (FK) y
    // restricciones. También carga los datos iniciales (seed).
    // ---------------------------------------------------------------------
    public class ContextoBolsines : DbContext
    {
        // Se asigna una sola vez al iniciar la aplicación (Program.cs),
        // a partir de "ConnectionStrings:Bolsines" en appsettings.json.
        public static string CadenaConexion { get; set; } = "Data Source=bolsines.db";

        // Tablas
        public DbSet<EstadoBD> Estados => Set<EstadoBD>();
        public DbSet<ComisionMedicaBD> ComisionesMedicas => Set<ComisionMedicaBD>();
        public DbSet<BolsinBD> Bolsines => Set<BolsinBD>();
        public DbSet<CambioEstadoBolsinBD> CambiosEstadoBolsin => Set<CambioEstadoBolsinBD>();
        public DbSet<UsuarioBD> Usuarios => Set<UsuarioBD>();
        public DbSet<RolBD> Roles => Set<RolBD>();
        public DbSet<EmpleadoBD> Empleados => Set<EmpleadoBD>();
        public DbSet<SesionBD> Sesiones => Set<SesionBD>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(CadenaConexion);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ----- Estado -----
            modelBuilder.Entity<EstadoBD>(tabla =>
            {
                tabla.ToTable("Estado");
                tabla.HasKey(e => e.Id);
                tabla.Property(e => e.Ambito).IsRequired().HasMaxLength(50);
                tabla.Property(e => e.Descripcion).HasMaxLength(200);
                tabla.Property(e => e.Nombre).IsRequired().HasMaxLength(50);
                tabla.HasIndex(e => new { e.Ambito, e.Nombre }).IsUnique();
            });

            // ----- ComisionMedica -----
            modelBuilder.Entity<ComisionMedicaBD>(tabla =>
            {
                tabla.ToTable("ComisionMedica");
                tabla.HasKey(cm => cm.Codigo);
                tabla.Property(cm => cm.Codigo).HasMaxLength(20);
                tabla.Property(cm => cm.Nombre).IsRequired().HasMaxLength(100);
                tabla.Property(cm => cm.Direccion).HasMaxLength(150);
                tabla.Property(cm => cm.Email).HasMaxLength(100);
                tabla.Property(cm => cm.Telefono).HasMaxLength(30);
            });

            // ----- Bolsin -----
            modelBuilder.Entity<BolsinBD>(tabla =>
            {
                tabla.ToTable("Bolsin");
                tabla.HasKey(b => b.NumeroBolsin);
                tabla.Property(b => b.NumeroBolsin).ValueGeneratedNever();
                tabla.HasIndex(b => b.NumeroPrecinto).IsUnique();

                // Dos FK a la misma tabla (origen y destino)
                tabla.HasOne<ComisionMedicaBD>()
                    .WithMany()
                    .HasForeignKey(b => b.CodigoCMOrigen)
                    .OnDelete(DeleteBehavior.Restrict);

                tabla.HasOne<ComisionMedicaBD>()
                    .WithMany()
                    .HasForeignKey(b => b.CodigoCMDestino)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ----- CambioEstadoBolsin -----
            modelBuilder.Entity<CambioEstadoBolsinBD>(tabla =>
            {
                tabla.ToTable("CambioEstadoBolsin");
                tabla.HasKey(ce => ce.Id);

                // Composición: si se borra el bolsín, se borran sus cambios de estado
                tabla.HasOne<BolsinBD>()
                    .WithMany()
                    .HasForeignKey(ce => ce.NumeroBolsin)
                    .OnDelete(DeleteBehavior.Cascade);

                tabla.HasOne<EstadoBD>()
                    .WithMany()
                    .HasForeignKey(ce => ce.IdEstado)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ----- Usuario -----
            modelBuilder.Entity<UsuarioBD>(tabla =>
            {
                tabla.ToTable("Usuario");
                tabla.HasKey(u => u.Id);
                tabla.Property(u => u.Nombre).IsRequired().HasMaxLength(50);
                tabla.Property(u => u.Contrasena).IsRequired().HasMaxLength(100);
                tabla.HasIndex(u => u.Nombre).IsUnique();
            });

            // ----- Rol -----
            modelBuilder.Entity<RolBD>(tabla =>
            {
                tabla.ToTable("Rol");
                tabla.HasKey(r => r.Id);
                tabla.Property(r => r.Nombre).IsRequired().HasMaxLength(50);
                tabla.Property(r => r.Descripcion).HasMaxLength(200);
                tabla.HasIndex(r => r.Nombre).IsUnique();
            });

            // ----- Empleado -----
            modelBuilder.Entity<EmpleadoBD>(tabla =>
            {
                tabla.ToTable("Empleado");
                tabla.HasKey(e => e.Id);
                tabla.Property(e => e.Nombre).IsRequired().HasMaxLength(50);
                tabla.Property(e => e.Apellido).IsRequired().HasMaxLength(50);
                tabla.Property(e => e.Email).HasMaxLength(100);

                // Un usuario pertenece a un solo empleado (1 a 1)
                tabla.HasIndex(e => e.IdUsuario).IsUnique();

                tabla.HasOne<UsuarioBD>()
                    .WithMany()
                    .HasForeignKey(e => e.IdUsuario)
                    .OnDelete(DeleteBehavior.Restrict);

                tabla.HasOne<RolBD>()
                    .WithMany()
                    .HasForeignKey(e => e.IdRol)
                    .OnDelete(DeleteBehavior.Restrict);

                tabla.HasOne<ComisionMedicaBD>()
                    .WithMany()
                    .HasForeignKey(e => e.CodigoCMAsignada)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ----- Sesion -----
            modelBuilder.Entity<SesionBD>(tabla =>
            {
                tabla.ToTable("Sesion");
                tabla.HasKey(s => s.Id);

                tabla.HasOne<UsuarioBD>()
                    .WithMany()
                    .HasForeignKey(s => s.IdUsuario)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Datos iniciales (los que antes estaban en DatosPrueba)
            DatosIniciales.Cargar(modelBuilder);
        }
    }
}
