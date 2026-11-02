namespace CapaDatos
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Habitacion")]
    public partial class Habitacion
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Habitacion()
        {
            DetalleReserva = new HashSet<DetalleReserva>();
        }

        [Key]
        public int IdHabitacion { get; set; }

        [Required]
        [StringLength(10)]
        public string NumeroHabitacion { get; set; }

        public int Piso { get; set; }

        public int IdTipoHabitacion { get; set; }

        public int IdEstadoHabitacion { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DetalleReserva> DetalleReserva { get; set; }

        public virtual EstadoHabitacion EstadoHabitacion { get; set; }

        public virtual TipoHabitacion TipoHabitacion { get; set; }
    }
}
