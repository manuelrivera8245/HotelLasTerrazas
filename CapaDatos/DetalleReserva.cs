namespace CapaDatos
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DetalleReserva")]
    public partial class DetalleReserva
    {
        [Key]
        public int IdDetalleReserva { get; set; }

        public int IdReserva { get; set; }

        public int IdHabitacion { get; set; }

        public decimal PrecioNocheAplicado { get; set; }

        public virtual Habitacion Habitacion { get; set; }

        public virtual Reserva Reserva { get; set; }
    }
}
