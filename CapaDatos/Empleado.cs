namespace CapaDatos
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Empleado")]
    public partial class Empleado
    {
        [Key]
        public int IdEmpleado { get; set; }

        public int IdUsuario { get; set; }

        [Required]
        [StringLength(20)]
        public string NumeroDocumento { get; set; }

        [Required]
        [StringLength(80)]
        public string Nombres { get; set; }

        [Required]
        [StringLength(80)]
        public string Apellidos { get; set; }

        [StringLength(20)]
        public string Telefono { get; set; }

        [Required]
        [StringLength(40)]
        public string Cargo { get; set; }

        [Column(TypeName = "date")]
        public DateTime FechaIngreso { get; set; }

        public virtual Usuario Usuario { get; set; }
    }
}
