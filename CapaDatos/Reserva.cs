namespace CapaDatos
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Reserva")]
    public partial class Reserva
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Reserva()
        {
            DetalleReserva = new HashSet<DetalleReserva>();
            Pago = new HashSet<Pago>();
        }

        [Key]
        public int IdReserva { get; set; }

        public int IdCliente { get; set; }

        public DateTime FechaReserva { get; set; }

        [Column(TypeName = "date")]
        public DateTime FechaEntrada { get; set; }

        [Column(TypeName = "date")]
        public DateTime FechaSalida { get; set; }

        public int IdEstadoReserva { get; set; }

        public decimal MontoTotal { get; set; }

        [StringLength(200)]
        public string MotivoCancelacion { get; set; }

        public virtual Cliente Cliente { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DetalleReserva> DetalleReserva { get; set; }

        public virtual EstadoReserva EstadoReserva { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Pago> Pago { get; set; }
    }
}
