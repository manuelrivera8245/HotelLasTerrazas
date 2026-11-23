using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace CapaDatos
{
    public partial class HotelContext : DbContext
    {
        public HotelContext()
            : base("name=HotelConnection")
        {
        }

        public virtual DbSet<Cliente> Cliente { get; set; }
        public virtual DbSet<Comprobante> Comprobante { get; set; }
        public virtual DbSet<DetalleReserva> DetalleReserva { get; set; }
        public virtual DbSet<Empleado> Empleado { get; set; }
        public virtual DbSet<EstadoHabitacion> EstadoHabitacion { get; set; }
        public virtual DbSet<EstadoReserva> EstadoReserva { get; set; }
        public virtual DbSet<Habitacion> Habitacion { get; set; }
        public virtual DbSet<MetodoPago> MetodoPago { get; set; }
        public virtual DbSet<Pago> Pago { get; set; }
        public virtual DbSet<Reserva> Reserva { get; set; }
        public virtual DbSet<Rol> Rol { get; set; }
        public virtual DbSet<Tarifa> Tarifa { get; set; }
        public virtual DbSet<Temporada> Temporada { get; set; }
        public virtual DbSet<TipoComprobante> TipoComprobante { get; set; }
        public virtual DbSet<TipoHabitacion> TipoHabitacion { get; set; }
        public virtual DbSet<Usuario> Usuario { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cliente>()
                .Property(e => e.TipoDocumento)
                .IsUnicode(false);

            modelBuilder.Entity<Cliente>()
                .Property(e => e.NumeroDocumento)
                .IsUnicode(false);

            modelBuilder.Entity<Cliente>()
                .Property(e => e.Nombres)
                .IsUnicode(false);

            modelBuilder.Entity<Cliente>()
                .Property(e => e.Apellidos)
                .IsUnicode(false);

            modelBuilder.Entity<Cliente>()
                .Property(e => e.Telefono)
                .IsUnicode(false);

            modelBuilder.Entity<Cliente>()
                .Property(e => e.Correo)
                .IsUnicode(false);

            modelBuilder.Entity<Cliente>()
                .Property(e => e.Direccion)
                .IsUnicode(false);

            modelBuilder.Entity<Cliente>()
                .HasMany(e => e.Reserva)
                .WithRequired(e => e.Cliente)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Comprobante>()
                .Property(e => e.Serie)
                .IsUnicode(false);

            modelBuilder.Entity<Comprobante>()
                .Property(e => e.Numero)
                .IsUnicode(false);

            modelBuilder.Entity<DetalleReserva>()
                .Property(e => e.PrecioNocheAplicado)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Empleado>()
                .Property(e => e.NumeroDocumento)
                .IsUnicode(false);

            modelBuilder.Entity<Empleado>()
                .Property(e => e.Nombres)
                .IsUnicode(false);

            modelBuilder.Entity<Empleado>()
                .Property(e => e.Apellidos)
                .IsUnicode(false);

            modelBuilder.Entity<Empleado>()
                .Property(e => e.Telefono)
                .IsUnicode(false);

            modelBuilder.Entity<Empleado>()
                .Property(e => e.Cargo)
                .IsUnicode(false);

            modelBuilder.Entity<EstadoHabitacion>()
                .Property(e => e.NombreEstado)
                .IsUnicode(false);

            modelBuilder.Entity<EstadoHabitacion>()
                .HasMany(e => e.Habitacion)
                .WithRequired(e => e.EstadoHabitacion)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<EstadoReserva>()
                .Property(e => e.NombreEstado)
                .IsUnicode(false);

            modelBuilder.Entity<EstadoReserva>()
                .HasMany(e => e.Reserva)
                .WithRequired(e => e.EstadoReserva)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Habitacion>()
                .Property(e => e.NumeroHabitacion)
                .IsUnicode(false);

            modelBuilder.Entity<Habitacion>()
                .HasMany(e => e.DetalleReserva)
                .WithRequired(e => e.Habitacion)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<MetodoPago>()
                .Property(e => e.NombreMetodo)
                .IsUnicode(false);

            modelBuilder.Entity<MetodoPago>()
                .HasMany(e => e.Pago)
                .WithRequired(e => e.MetodoPago)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Pago>()
                .Property(e => e.Monto)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Pago>()
                .HasMany(e => e.Comprobante)
                .WithRequired(e => e.Pago)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Reserva>()
                .Property(e => e.MontoTotal)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Reserva>()
                .Property(e => e.MotivoCancelacion)
                .IsUnicode(false);

            modelBuilder.Entity<Reserva>()
                .HasMany(e => e.DetalleReserva)
                .WithRequired(e => e.Reserva)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Reserva>()
                .HasMany(e => e.Pago)
                .WithRequired(e => e.Reserva)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Rol>()
                .Property(e => e.NombreRol)
                .IsUnicode(false);

            modelBuilder.Entity<Rol>()
                .HasMany(e => e.Usuario)
                .WithRequired(e => e.Rol)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Tarifa>()
                .Property(e => e.PrecioNoche)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Temporada>()
                .Property(e => e.NombreTemporada)
                .IsUnicode(false);

            modelBuilder.Entity<Temporada>()
                .HasMany(e => e.Tarifa)
                .WithRequired(e => e.Temporada)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TipoComprobante>()
                .Property(e => e.NombreTipo)
                .IsUnicode(false);

            modelBuilder.Entity<TipoComprobante>()
                .HasMany(e => e.Comprobante)
                .WithRequired(e => e.TipoComprobante)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TipoHabitacion>()
                .Property(e => e.NombreTipo)
                .IsUnicode(false);

            modelBuilder.Entity<TipoHabitacion>()
                .Property(e => e.Descripcion)
                .IsUnicode(false);

            modelBuilder.Entity<TipoHabitacion>()
                .HasMany(e => e.Habitacion)
                .WithRequired(e => e.TipoHabitacion)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TipoHabitacion>()
                .HasMany(e => e.Tarifa)
                .WithRequired(e => e.TipoHabitacion)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Usuario>()
                .Property(e => e.NombreUsuario)
                .IsUnicode(false);

            modelBuilder.Entity<Usuario>()
                .Property(e => e.ClaveHash)
                .IsUnicode(false);

            modelBuilder.Entity<Usuario>()
                .HasMany(e => e.Cliente)
                .WithRequired(e => e.Usuario)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Usuario>()
                .HasMany(e => e.Empleado)
                .WithRequired(e => e.Usuario)
                .WillCascadeOnDelete(false);
        }
    }
}
