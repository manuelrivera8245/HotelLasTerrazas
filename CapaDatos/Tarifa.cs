namespace CapaDatos
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Tarifa")]
    public partial class Tarifa
    {
        [Key]
        public int IdTarifa { get; set; }

        public int IdTipoHabitacion { get; set; }

        public int IdTemporada { get; set; }

        public decimal PrecioNoche { get; set; }

        public virtual Temporada Temporada { get; set; }

        public virtual TipoHabitacion TipoHabitacion { get; set; }
    }
}
