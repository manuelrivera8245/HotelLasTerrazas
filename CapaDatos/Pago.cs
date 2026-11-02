namespace CapaDatos
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Pago")]
    public partial class Pago
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Pago()
        {
            Comprobante = new HashSet<Comprobante>();
        }

        [Key]
        public int IdPago { get; set; }

        public int IdReserva { get; set; }

        public int IdMetodoPago { get; set; }

        public decimal Monto { get; set; }

        public DateTime FechaPago { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Comprobante> Comprobante { get; set; }

        public virtual MetodoPago MetodoPago { get; set; }

        public virtual Reserva Reserva { get; set; }
    }
}
