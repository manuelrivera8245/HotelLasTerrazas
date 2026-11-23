namespace CapaDatos
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Comprobante")]
    public partial class Comprobante
    {
        [Key]
        public int IdComprobante { get; set; }

        public int IdPago { get; set; }

        public int IdTipoComprobante { get; set; }

        [Required]
        [StringLength(10)]
        public string Serie { get; set; }

        [Required]
        [StringLength(15)]
        public string Numero { get; set; }

        public DateTime FechaEmision { get; set; }

        public virtual Pago Pago { get; set; }

        public virtual TipoComprobante TipoComprobante { get; set; }
    }
}
