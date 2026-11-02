namespace CapaDatos
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TipoHabitacion")]
    public partial class TipoHabitacion
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TipoHabitacion()
        {
            Habitacion = new HashSet<Habitacion>();
            Tarifa = new HashSet<Tarifa>();
        }

        [Key]
        public int IdTipoHabitacion { get; set; }

        [Required]
        [StringLength(40)]
        public string NombreTipo { get; set; }

        public int Capacidad { get; set; }

        [StringLength(250)]
        public string Descripcion { get; set; }

        public bool Estado { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Habitacion> Habitacion { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tarifa> Tarifa { get; set; }
    }
}
